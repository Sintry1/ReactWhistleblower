import "./Reports.css"
import { useEffect, useState } from "react";

export default function Reports() {
  const [reports, setReports] = useState([]);

  useEffect(() => {
    // Fetch reports from the database
    const fetchReports = async () => {
      try {
        const response = await fetch("/api/reports");
        const data = await response.json();
        setReports(data);
      } catch (error) {
        console.error("Error fetching reports:", error);
      }
    };

    fetchReports();
  }, []);
  

  const decryptValue = async (encryption, masterKey) => {
    try {
      const keyMaterial = await crypto.subtle.exportKey("raw", masterKey);

      const key = await crypto.subtle.deriveKey(
        {
          name: "PBKDF2",
          salt: new TextEncoder().encode(localStorage.getItem("Salt")),
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
