import axios from "axios";
import { useEffect, useState } from "react";
import "./Reports.css";

export default function Reports() {
  const [reports, setReports] = useState([]);

  const host = "http://localhost:5090/";

////// FIGURE OUT HOW TO PASS THE INDUSTRY TO THE REPORTS PAGE AFTER LOGIN

const fetchReports = async () => {
  try {
    const response = await axios.get(
      `${host}api/Report/retrieveReports/Information Technology`,
      {}
    );
    console.log(response.data);
  } catch (error) {
    console.error("Error fetching reports:", error);
  }
};

  useEffect(() => {
    // Fetch reports from the database
    

    fetchReports();
  }, []);

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

    const salt = crypto.getRandomValues(new Uint8Array(16));

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
        salt: salt,
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

  const decryptValue = async (encryption, decryptionKey) => {
    try {
      const keyMaterial = await crypto.subtle.exportKey("raw", decryptionKey);

      const key = await crypto.subtle.deriveKey(
        {
          name: "PBKDF2",
          salt: ""/* await fetch(`${host}api/Regulator/GetSalt/${industry}`, {});*/,
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
      const encryptedPassword = new Uint8Array(encryption.password);

      const decryptedPasswordBuffer = await crypto.subtle.decrypt(
        { name: "AES-GCM", iv: iv },
        key,
        encryptedPassword
      );

      // Convert the decrypted password ArrayBuffer to a string
      const decryptedPasswordString = new TextDecoder().decode(
        decryptedPasswordBuffer
      );

      console.log("Decrypted Password String:", decryptedPasswordString);

      return decryptedPasswordString;
    } catch (error) {
      console.error("Error during decryption:", error);
      throw error;
    }
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
          {reports.map((report) => (
            <tr key={report.id}>
              <td>{report.industry}</td>
              <td>{report.employer}</td>
              <td>{report.description}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
