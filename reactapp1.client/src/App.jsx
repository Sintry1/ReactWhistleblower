/* eslint-disable no-unused-vars */
import { Route, Routes } from "react-router-dom";
import "./App.css";
import Information from "./pages/Information";
import LandingPage from "./pages/LandingPage";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Reports from "./pages/Reports";
import SendReport from "./pages/SendReport";

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<LandingPage />} />
      <Route path="/information" element={<Information />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/reports" element={<Reports />} />
      <Route path="/sendreport" element={<SendReport />} />
    </Routes>
  );
}
