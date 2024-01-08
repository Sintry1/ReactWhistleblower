/* eslint-disable no-unused-vars */
import { Route, Routes, Navigate } from "react-router-dom";
import "./App.css";
import Information from "./pages/Information";
import LandingPage from "./pages/LandingPage";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Reports from "./pages/Reports";
import SendReport from "./pages/SendReport";

function PrivateRoute({ children }) {
  const isAuth = sessionStorage.getItem("User") && sessionStorage.getItem("Industry"); // check if User and Industry exist in sessionStorage
  return isAuth ? children : <Navigate to="/login" replace />;
}

export default function App() {
  const isAuth = false;
  return (
    <Routes>
      <Route path="/" element={<LandingPage />} />
      <Route path="/information" element={<Information />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/reports" element={<PrivateRoute><Reports /></PrivateRoute>} />
      <Route path="/sendreport" element={<SendReport />} />
    </Routes>
  );
}
