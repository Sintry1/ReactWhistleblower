import { useState } from "react";
import { Link } from "react-router-dom";
import bcrypt from "bcryptjs";

export default function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [industry, setIndustry] = useState("");

  const host = "http://localhost:5090/";

  const checkPassword = async (name, password) => {
    name = username;

    const storedPassword = await fetch(
      `${host}api/Regulator/passwordCheck/${name}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      }
    );
    return bcrypt.compareSync(password, storedPassword);
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
    console.log(
      "currentUsername inside checkUserExists: ",
      currentUserData.userName
    );
    const encryptionKey = await deriveKey(industry);

    const decryptData = {
      iv: Uint8Array.from(atob(currentUserData.iv), (c) => c.charCodeAt(0)),
      username: Uint8Array.from(atob(currentUserData.userName), (c) =>
        c.charCodeAt(0)
      ),
    };
    console.log("IV length:", decryptData.iv.length);
    console.log("Username length:", decryptData.username.length);
    const decryptedUsername = await decryptValue(
      decryptData,
      encryptionKey
    );
    console.log(
      "Decrypted Username inside checkUserExists: ",
      decryptedUsername
    );

    const response = await fetch(
      `${host}api/Regulator/userExists/${industry}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      }
    );
    if (!response.ok) {
      return false;
    }

    console.log("Response inside checkUserExists: ", await response.json());
    const data = await response.json();
    console.log("Data inside checkUserExists: ", data);
    return data;
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

  const decryptValue = async (encryption, decryptionKey) => {
    try {
      const keyMaterial = await crypto.subtle.exportKey("raw", decryptionKey);

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
      const encryptedUsername = new Uint8Array(encryption.username);

      const decryptedUsernameBuffer = await crypto.subtle.decrypt(
        { name: "AES-GCM", iv: iv },
        key,
        encryptedUsername
      );

      // Convert the decrypted password ArrayBuffer to a string
      const decryptedUsernameString = new TextDecoder().decode(
        decryptedUsernameBuffer
      );

      return decryptedUsernameString;
    } catch (error) {
      console.error("Error during decryption:", error);
      throw error;
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    let encryptionKey = await deriveKey(industry);

    let salt = await fetch(`${host}api/Regulator/GetRegulatorSalt/${industry}`, {
      method: "GET",
      headers: { "Content-Type": "application/json" },
    });

    // let encryptedUsernameString = btoa(
    //   String.fromCharCode.apply(null, encryptedUsername.input)
    // );

    let data = await checkUserExists(industry);
    console.log(data);

    try {
      // Check if user exists
      // if (!checkUserExists(encryptedUsername)) {
      //   throw new Error("User does not exist");
      // }
      // const userExistsResponse = await fetch(
      //   `${host}api/Regulator/userExists/${username}`
      // );
      // if (!userExistsResponse.data.UserExists) {
      //   throw new Error("User does not exist");
      // }
      // Check if industry matches
      // const industryMatchesResponse = await axios.get(
      //   `${host}api/Regulator/checkIndustry/${username}/${industry}`
      // );
      // if (!industryMatchesResponse.data.IndustryMatches) {
      //   throw new Error("Industry does not match");
      // }
      // Check if password matches
      // Assuming you have an endpoint for this
      // const passwordMatchesResponse = await checkPassword(username, password);
      // if (!passwordMatchesResponse) {
      //   throw new Error("Password does not match");
      // }
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
