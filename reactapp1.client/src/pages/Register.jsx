import { useState } from "react";
import bcrypt from "bcryptjs";

export default function Register() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [repeatPassword, setRepeatPassword] = useState("");
  const [industry, setIndustry] = useState("");

  const salt = bcrypt.genSaltSync(10);

  const hashPassword = (password) => {
    const hashedPassword = bcrypt.hashSync(password, salt);
    return hashedPassword;
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
    const hashedPassword = hashPassword(password);
    let response;
    try {response = await fetch("http://localhost:5090/api/Regulator/createRegulator", {
      method: "POST",
      body: JSON.stringify({
        Username: email,
        HashedPassword: hashedPassword,
        IndustryName: industry,
      }),
      headers: {
        "Content-Type": "application/json",
      },
    });
  } catch (err) {
    console.error('Network error:', err);
    return
  }
  if (!response.ok){
    console.error('Response error:', response.status);
    return;
  }
    const data = await response.json();
    return data;
  };

  const handleRegister = (e) => {
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
