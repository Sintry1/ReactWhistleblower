import { useEffect, useState } from "react";
import "./SendReport.css";

export default function SendReport() {
  const host = "http://localhost:5090/";
  useEffect(() => {}, []);

  const [industry, setIndustry] = useState("");
  const [companyName, setCompanyName] = useState("");
  const [reportDetails, setReportDetails] = useState("");
  const [email, setEmail] = useState("");

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

  const encryptValue = async (input, encryptionKey) => {
    try {
      const keyMaterial = await crypto.subtle.exportKey("raw", encryptionKey);

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
      const iv = crypto.getRandomValues(new Uint8Array(16));

      const data = new TextEncoder().encode(input);

      const encryptedDataBuffer = await crypto.subtle.encrypt(
        { name: "AES-GCM", iv: iv },
        key,
        data
      );

      // Convert the encrypted data ArrayBuffer to a Uint8Array
      const encryptedData = new Uint8Array(encryptedDataBuffer);

      return {
        iv: Array.from(iv),
        data: Array.from(encryptedData),
      };
    } catch (error) {
      console.error("Error during encryption:", error);
      console.error("Error name:", error.name);
      console.error("Error message:", error.message);
      throw error;
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    let encryptionKey = await deriveKey(industry);
    let encryptedReport = await encryptValue(reportDetails, encryptionKey);
    let ecnryptedCompany = await encryptValue(companyName, encryptionKey);
    let encryptedReportString = btoa(
      String.fromCharCode.apply(null, encryptedReport.data)
    );
    let encryptedReportIv = btoa(
      String.fromCharCode.apply(null, encryptedReport.iv)
    );
    let encryptedCompanyString = btoa(
      String.fromCharCode.apply(null, ecnryptedCompany.data)
    );
    let encryptedCompanyIv = btoa(
      String.fromCharCode.apply(null, ecnryptedCompany.iv)
    );

    if (industry === ""){
      alert("Please select an industry");
      return;
    }

    if (companyName === ""){
      alert("Please enter a company name");
      return;
    }

    if (reportDetails === ""){
      alert("Please enter a report description");
      return;
    }

    // Send report using Axios
    await fetch(`${host}api/Report/sendReport`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        IndustryName: industry,
        CompanyName: encryptedCompanyString,
        CompanyIv: encryptedCompanyIv,
        Description: encryptedReportString,
        DescriptionIv: encryptedReportIv,
        Email: email,
      }),
    })
      .then((res) => res.json());
  };

  return (
    <div className="container">
      <h2>Submit a Report</h2>
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="industry">Industry</label>
          <select
            id="industry"
            className="dropdown"
            onChange={(e) => setIndustry(e.target.value)}
          >
            <option value="">
              Select Industry
            </option>
            <option value="Information Technology">
              Information Technology
            </option>
            <option value="Financial Services">Financial Services</option>
            <option value="Healthcare">Healthcare</option>
            <option value="Law Enforcement">Law Enforcement</option>
            <option value="Leisure">Leisure</option>
            <option value="Hospitality">Hospitality</option>
          </select>
        </div>
        <div>
          <label htmlFor="company">Company Name</label>
          <input
            id="company"
            className="input"
            type="text"
            onChange={(e) => setCompanyName(e.target.value)}
            placeholder="Company Name"
          />
        </div>
        <div>
          <label htmlFor="description">Report Details</label>
          <textarea
            onChange={(e) => setReportDetails(e.target.value)}
            id="description"
            className="description"
            placeholder="Description"
          ></textarea>
        </div>
        <div>
          <label htmlFor="email">Email (Optional)</label>
          <input
            onChange={(e) => setEmail(e.target.value)}
            id="email"
            className="input"
            type="email"
            placeholder="Email (optional)"
          />
        </div>
        <button type="submit">Send</button>
      </form>
    </div>
  );
}
