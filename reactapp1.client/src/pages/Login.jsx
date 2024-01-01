import CryptoJS from "crypto-js";
import { useState } from "react";

export default function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const handleUsernameChange = (e) => {
    setUsername(e.target.value);
  };

  const handlePasswordChange = (e) => {
    setPassword(e.target.value);
  };

  const encryptValue = (value) => {
    const secretKey = "your-secret-key"; // Replace with your secret key
    const encryptedValue = CryptoJS.AES.encrypt(value, secretKey).toString();
    return encryptedValue;
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    const encryptedUsername = encryptValue(username);
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
        <button type="submit">Login</button>
      </form>
    </div>
  );
}
