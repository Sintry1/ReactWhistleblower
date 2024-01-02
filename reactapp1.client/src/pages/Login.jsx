import CryptoJS from "crypto-js";
import { useState } from "react";
import { Link } from "react-router-dom";
import bcrypt from "bcryptjs";
import axios from "axios"; 

export default function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const api = "http://localhost:5090/";

  const salt = bcrypt.genSaltSync(10);

  const checkPassword = async (name, password) => {
    const hashedPassword = bcrypt.hashSync(password, salt);
    const storedPassword = axios.get(`${api}regulators`, {});
    return bcrypt.compareSync(hashedPassword, storedPassword);
  };

  const handleUsernameChange = (e) => {
    setUsername(e.target.value);
  };

  const handlePasswordChange = (e) => {
    setPassword(e.target.value);
  };

  // const encryptValue = (value) => {
  //   const secretKey = "your-secret-key"; // Replace with your secret key
  //   const encryptedValue = CryptoJS.AES.encrypt(value, secretKey).toString();
  //   return encryptedValue;
  // };

  const handleSubmit = (e) => {
    e.preventDefault();
    const encryptedPassword = encryptValue(password);
    // Perform login logic here with encrypted values
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
        <button type="submit"><Link to="/reports">Login</Link></button>
      </form>
    </div>
  );
}
