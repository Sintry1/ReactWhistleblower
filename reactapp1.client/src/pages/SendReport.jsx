import { useEffect, useState } from "react";

export default function SendReport() {
  const [reports, setReports] = useState([]);

  useEffect(() => {
    // Fetch reports from the database
    const fetchReports = async () => {
      try {
        const response = await fetch("/api/reports");
        const data = await response.json();
        setReports(data);
      } catch (error) {
        console.error("Error fetching reports:", error);
      }
    };

    fetchReports();
  }, []);

  return (
    <div>
      <h1>Report Previews</h1>
      <table>
        <thead>
          <tr>
            <th>Date Submitted</th>
            <th>Resolved</th>
            <th>Preview</th>
            <th>Industry</th>
          </tr>
        </thead>
        <tbody>
          {reports.map((report) => (
            <tr key={report.id}>
              <td>{report.dateSubmitted}</td>
              <td>{report.resolved ? "Yes" : "No"}</td>
              <td>{report.preview}</td>
              <td>{report.industry}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
