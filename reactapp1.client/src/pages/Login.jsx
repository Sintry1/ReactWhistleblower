import bcrypt from "bcryptjs";
import { useState } from "react";
import { Link } from "react-router-dom";

export default function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [industry, setIndustry] = useState("");

  const host = "http://localhost:5090/";

  const checkPassword = async (name, password) => {
    
    let encryptedUsername = await encryptValue(username, await deriveKey(industry));
    console.log("Encrypted username: ", name);
    name = btoa(
      String.fromCharCode.apply(null, encryptedUsername.data)
    );

    const storedPassword = await fetch(
      `${host}api/Regulator/passwordCheck/${name}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      }
    );
    const data = await storedPassword.json();
    console.log("Stored Password: ", data);
    return bcrypt.compareSync(password, data.hashedPassword);
  };


  const checkUsernameMatch = (decryptedUsername, inputUsername) => {
    return decryptedUsername === inputUsername;
  };


  const checkUserExists = async (industry) => {
    const currentUser = await fetch(
      `${host}api/Regulator/GetIvAndUserName/${industry}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      }
    );
    if (!currentUser.ok) {
      return console.log("sumting wong");
    }
    const currentUserData = await currentUser.json();
    
    const encryptionKey = await deriveKey(industry);

    const decryptData = {
      iv: Uint8Array.from(atob(currentUserData.iv), (c) => c.charCodeAt(0)),
      username: Uint8Array.from(atob(currentUserData.userName), (c) =>
        c.charCodeAt(0)
      ),
    };
    let decryptedUsername;
    try {
      decryptedUsername = await decryptValue(decryptData, encryptionKey);
    } catch (error) {
      console.error("Error during decryption:", error);
      throw error; // or handle the error in some other way
    }

    return checkUsernameMatch(decryptedUsername, username);
  };

  const handleUsernameChange = (e) => {
    setUsername(e.target.value);
  };

  const handlePasswordChange = (e) => {
    setPassword(e.target.value);
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

  const decryptValue = async (encryption, decryptionKey) => {
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
      const encryptedData = new Uint8Array(encryption.username);

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
  
      const ivResponse = await fetch(
        `${host}api/Regulator/GetIvAndUserName/${industry}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
          },
        }
      );
      const ivData = await ivResponse.json();
      const iv = Uint8Array.from(atob(ivData.iv), (c) => c.charCodeAt(0));
  
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
      try {
      // Check if user exists
      if (!checkUserExists(industry)) {
        throw new Error("There was an error logging in, please try again");
      }
      // Check if industry matches
      // const industryMatchesResponse = await axios.get(
      //   `${host}api/Regulator/checkIndustry/${username}/${industry}`
      // );
      // if (!industryMatchesResponse.data.IndustryMatches) {
      //   throw new Error("Industry does not match");
      // }
      // Check if password matches
      const passwordMatchesResponse = await checkPassword(username, password);
      if (!passwordMatchesResponse) {
        throw new Error("There was an error logging in, please try again");
      }
      // Check is user exists
      // if user exists, check industry matches
      // if industry matches, check password
      // if password matches, login by redirecting to reports page
      // if password does not match, display error message
      // when redirected to reports page, pass industry and username as props (if secure)
      // const encryptedPassword = encryptValue(password);
      // Perform login logic here with encrypted values
    } catch (err) {
      console.log(err);
    }
  };

  return (
    <div>
      <h2>Login</h2>
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="username">Username:</label>
          <input
            type="text"
            id="username"
            value={username}
            onChange={handleUsernameChange}
          />
        </div>
        <div>
          <label htmlFor="password">Password:</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={handlePasswordChange}
          />
        </div>
        <select id="industry" onChange={(e) => setIndustry(e.target.value)}>
          <option value="">Select Industry</option>
          <option value="Information Technology">Information Technology</option>
          <option value="Financial Services">Financial Services</option>
          <option value="Healthcare">Healthcare</option>
          <option value="Law Enforcement">Law Enforcement</option>
          <option value="Leisure">Leisure</option>
          <option value="Hospitality">Hospitality</option>
        </select>
        <button type="submit">{/* <Link to="/reports">Login</Link> */}</button>
      </form>
      <button>
        <Link to="/register">Register</Link>
      </button>
    </div>
  );
}
