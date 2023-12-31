import React, { useState } from 'react';

export default function Register() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [repeatPassword, setRepeatPassword] = useState('');
  const [industry, setIndustry] = useState('');

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

  const handleRegister = () => {
    if (email === '' || password === '' || repeatPassword === '' || industry === '') {
      alert('Please fill in all fields');
      return;
    }

    if (password !== repeatPassword) {
      alert('Passwords do not match');
      return;
    }

    // Perform registration logic here
    // ...

    // Reset form fields
    setEmail('');
    setPassword('');
    setRepeatPassword('');
    setIndustry('');
  };

  return (
    <div>
      <input type="email" placeholder="Email" value={email} onChange={handleEmailChange} />
      <input type="password" placeholder="Password" value={password} onChange={handlePasswordChange} />
      <input type="password" placeholder="Repeat Password" value={repeatPassword} onChange={handleRepeatPasswordChange} />
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
      <button onClick={handleRegister}>Register</button>
    </div>
  );
};
