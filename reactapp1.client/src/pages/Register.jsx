import { useState } from "react";
import bcrypt from "bcryptjs";

export default function Register() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [repeatPassword, setRepeatPassword] = useState("");
  const [industry, setIndustry] = useState("");

  const host = "http://localhost:5090/";

  const hashPassword = (password) => {
    const salt = bcrypt.genSaltSync(10);
    const hashedPassword = bcrypt.hashSync(password, salt);
    return hashedPassword;
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

    const salt = crypto.getRandomValues(new Uint8Array(16));
    localStorage.setItem("Salt", salt);

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

  const handleEmailChange = (e) => {
    setEmail(e.target.value);
  };

  const handlePasswordChange = (e) => {
    setPassword(e.target.value);
  };

  const handleRepeatPasswordChange = (e) => {
    setRepeatPassword(e.target.value);
  };

  const handleIndustryChange = (e) => {
    setIndustry(e.target.value);
  };

  const registerRegulator = async (email, password, industry) => {
    console.log(`IV for: ${email}`, );
    const hashedPassword = hashPassword(password);
    let encryptionKey = await deriveKey(industry);
    let encryptedUsername = await encryptValue(email, encryptionKey);
    console.log(`IV for: ${email}`, encryptedUsername.iv);
    console.log(`Encrypted username for: ${email}`, encryptedUsername.input);
    let encryptedUsernameString = btoa(
      String.fromCharCode.apply(null, encryptedUsername.input)
    );
    let encryptedUsernameIv = btoa(
      String.fromCharCode.apply(null, encryptedUsername.iv)
    );
    console.log("String representation of Username: ", encryptedUsernameString);

    let response;
    try {
      response = await fetch(`${host}api/Regulator/createRegulator`, {
        method: "POST",
        body: JSON.stringify({
          Username: encryptedUsernameString,
          HashedPassword: hashedPassword,
          IndustryName: industry,
          Iv: encryptedUsernameIv,
        }),
        headers: {
          "Content-Type": "application/json",
        },
      });
    } catch (err) {
      console.error("Network error:", err);
      return;
    }
    if (!response.ok) {
      console.error("Response error:", response.status);
      return;
    }
    const data = await response.json();
    return data;
  };

  const handleRegister = async (e) => {
    e.preventDefault();

    if (
      email === "" ||
      password === "" ||
      repeatPassword === "" ||
      industry === ""
    ) {
      alert("Please fill in all fields");
      return;
    }

    if (password !== repeatPassword) {
      alert("Passwords do not match");
      return;
    }

    registerRegulator(email, password, industry);

    // Reset form fields
    setEmail("");
    setPassword("");
    setRepeatPassword("");
    setIndustry("");
  };

  return (
    <div>
      <form onSubmit={handleRegister}>
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={handleEmailChange}
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={handlePasswordChange}
        />
        <input
          type="password"
          placeholder="Repeat Password"
          value={repeatPassword}
          onChange={handleRepeatPasswordChange}
        />
        <select value={industry} onChange={handleIndustryChange}>
          <option value="">Select Industry</option>
          <option value="Information Technology">Information Technology</option>
          <option value="Financial Services">Financial Services</option>
          <option value="Healthcare">Healthcare</option>
          <option value="Law Enforcement">Law Enforcement</option>
          <option value="Leisure">Leisure</option>
          <option value="Hospitality">Hospitality</option>
          {/* Add more options as needed */}
        </select>
        <button type="submit">Register</button>
      </form>
    </div>
  );
}
