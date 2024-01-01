import { Link } from "react-router-dom";

export default function LandingPage() {
  return (
    <div>
      <h1>Welcome to the Whistleblower App</h1>
      <p>
        This application is designed to provide a secure platform for reporting
        and exposing wrongdoing within organizations.
      </p>
      <p>
        We do not record or store any of your personal data. It is important
        that this process remains anonymous and safe.
      </p>
      <button>
        <Link to="/information">Information</Link>
      </button>
    </div>
  );
}
