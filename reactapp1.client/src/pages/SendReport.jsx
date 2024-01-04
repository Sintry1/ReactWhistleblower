import { useEffect, useState } from "react";
import "./SendReport.css";
import axios from "axios";

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

  const encryptValue = async (input, encryptionKey) => {
    const keyMaterial = await crypto.subtle.exportKey("raw", encryptionKey);

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

    const iv = crypto.getRandomValues(new Uint8Array(16));
    const cipher = await crypto.subtle.encrypt(
      { name: "AES-GCM", iv: iv },
      key,
      new TextEncoder().encode(input)
    );

    return {
      iv: iv,
      input: new Uint8Array(cipher),
    };
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    let encryptionKey = await deriveKey(industry);
    let encryptedReport = await encryptValue(reportDetails, encryptionKey);
    let ecnryptedCompany = await encryptValue(companyName, encryptionKey);
    let encryptedReportString = btoa(
      String.fromCharCode.apply(null, encryptedReport.input)
    );
    let ecnryptedCompanyString = btoa(
      String.fromCharCode.apply(null, ecnryptedCompany.input)
    );

    // Send report using Axios
    await fetch("http://localhost:5090/api/Report/sendReport", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        IndustryName: industry,
        CompanyName: ecnryptedCompanyString,
        Description: encryptedReportString,
        Email: email,
      }),
    })
      .then((res) => res.json())
      .then((data) => console.log(data))
      .catch((err) => console.log("dlkfngjo", err));
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
