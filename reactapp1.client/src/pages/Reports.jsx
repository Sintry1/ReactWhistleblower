import { useEffect, useState } from "react";
import "./Reports.css";

export default function Reports() {
  const [decryptedReports, setDecryptedReports] = useState([]);

  useEffect(() => {
    // Add event listener for beforeunload
    window.addEventListener("beforeunload", () => {
      // Clears sessionStorage on any sort of navigation away from the page, so that the user has to log in again whenever they navigate away.
      sessionStorage.clear();
    });

    // Fetch reports from the database
    fetchReports();

    // Cleanup
    return () => {
      // Remove event listener when the component unmounts
      window.removeEventListener("beforeunload", () => {
      });
    };
  }, []);

  const host = "http://localhost:5090/";

  const fetchReports = async () => {
    const industry = sessionStorage.getItem("Industry");
    const user = sessionStorage.getItem("User");
    try {
      const response = await fetch(
        `${host}api/Report/getReports/${industry}`,
        {
          method: "GET",
          headers: {
            "name-Header": user,
            "Content-Type": "application/json",
          },
        }
      );
      const data = await response.json();
      decryptReports(data.reports);
    } catch (error) {
      console.error("Error fetching reports:", error);
    }
  };

  const deriveKey = async (industry) => {
    let key;
    switch (industry) {
      case "Information Technology":
        key = import.meta.env.VITE_IT_SECRET_KEY;
        break;
      case "Financial Services":
        key = import.meta.env.VITE_FINSERV_SECRET_KEY;
        break;
      case "Healthcare":
        key = import.meta.env.VITE_HEALTHCARE_SECRET_KEY;
        break;
      case "Law Enforcement":
        key = import.meta.env.VITE_LAWENF_SECRET_KEY;
        break;
      case "Leisure":
        key = import.meta.env.VITE_LEISURE_SECRET_KEY;
        break;
      case "Hospitality":
        key = import.meta.env.VITE_HOSPITALITY_SECRET_KEY;
        break;
      default:
        break;
    }

    const salt = await fetch(
      `${host}api/Regulator/GetRegulatorSalt/${industry}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      }
    );
    const saltData = await salt.json();

    const encodedKey = new TextEncoder().encode(key);

    const keyMat = await crypto.subtle.importKey(
      "raw",
      encodedKey,
      { name: "PBKDF2" },
      false,
      ["deriveBits", "deriveKey"]
    );

    const derivedKey = await crypto.subtle.deriveKey(
      {
        name: "PBKDF2",
        salt: new Uint8Array(
          atob(saltData.salt)
            .split("")
            .map((char) => char.charCodeAt(0))
        ),
        iterations: 100000,
        hash: { name: "SHA-256" },
      },
      keyMat,
      { name: "AES-GCM", length: 256 },
      true,
      ["encrypt", "decrypt"]
    );

    return derivedKey;
  };

  const decryptValue = async (encryption, industry) => {
    const decryptionKey = await deriveKey(industry);
    try {
      const keyMaterial = await crypto.subtle.exportKey("raw", decryptionKey);
      const salt = await fetch(
        `${host}api/Regulator/GetRegulatorSalt/${industry}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
          },
        }
      );
      const saltData = await salt.json();

      const key = await crypto.subtle.deriveKey(
        {
          name: "PBKDF2",
          salt: new Uint8Array(
            atob(saltData.salt)
              .split("")
              .map((char) => char.charCodeAt(0))
          ),
          iterations: 100000,
          hash: { name: "SHA-256" },
        },
        await crypto.subtle.importKey(
          "raw",
          keyMaterial,
          { name: "PBKDF2" },
          false,
          ["deriveKey"]
        ),
        { name: "AES-GCM", length: 256 },
        true,
        ["encrypt", "decrypt"]
      );

      const iv = new Uint8Array(encryption.iv);
      const encryptedData = new Uint8Array(encryption.input);

      const decryptedDataBuffer = await crypto.subtle.decrypt(
        { name: "AES-GCM", iv: iv },
        key,
        encryptedData
      );

      // Convert the decrypted password ArrayBuffer to a string
      const decryptedDataString = new TextDecoder().decode(decryptedDataBuffer);

      return decryptedDataString;
    } catch (error) {
      console.error("Error during decryption:", error);
      console.error("Error name:", error.name);
      console.error("Error message:", error.message);
      throw error;
    }
  };

  const decryptReports = async (reports) => {
    const decryptedReports = [];
    for (const report of reports) {
      const companyDecrypt = {
        iv: Uint8Array.from(atob(report.companyIv), (c) => c.charCodeAt(0)),
        input: Uint8Array.from(atob(report.companyName), (c) =>
          c.charCodeAt(0)
        ),
      };
      const decryptedCompanyName = await decryptValue(
        companyDecrypt,
        report.industryName
      );
      const descriptionDecrypt = {
        iv: Uint8Array.from(atob(report.descriptionIv), (c) => c.charCodeAt(0)),
        input: Uint8Array.from(atob(report.description), (c) =>
          c.charCodeAt(0)
        ),
      };
      const decryptedDescription = await decryptValue(
        descriptionDecrypt,
        report.industryName
      );
      const decryptedReport = {
        id: report.reportID,
        industryName: report.industryName,
        companyName: decryptedCompanyName,
        description: decryptedDescription,
      };
      decryptedReports.push(decryptedReport);
    }
    setDecryptedReports(decryptedReports);
  };

  return (
    <div>
      <table className="table">
        <thead>
          <tr>
            <th className="narrowcolumn">Industry</th>
            <th className="narrowcolumn">Employer</th>
            <th className="widecolumn">Description</th>
          </tr>
        </thead>
        <tbody>
          {decryptedReports.map((report) => (
            <tr key={report.id}>
              <td className="column">{report.industryName}</td>
              <td className="column">{report.companyName}</td>
              <td className="column">{report.description}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
