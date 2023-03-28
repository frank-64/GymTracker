import React, { useState } from "react";
import Button from "react-bootstrap/Button";
import "./Navbar.css";

function Navbar(props) {
  const dateOptions = {
    weekday: "long",
    day: "numeric",
    month: "long",
    year: "numeric",
  };

  const timeOptions = { timeZoneName: "short" };

  // Initially setting the date and time used in the Navbar
  const [time, setTime] = useState(
    new Date().toLocaleTimeString("en-GB", timeOptions)
  );
  const date = new Date().toLocaleDateString("en-GB", dateOptions);

  // Update the time every 1000ms
  setInterval(
    () => setTime(new Date().toLocaleTimeString("en-GB", timeOptions)),
    1000
  );

  const logout = () => {
    if (props.logout) {
      localStorage.clear("authToken");
    }
  };

  return (
    <div className="navbar">
      <div className="navbar-left">
        <Button
          onClick={() => {
            window.location.href = props.navigateTarget;
            logout();
          }}
          variant="outline-light"
          style={{
            borderRadius: "0.5",
            fontSize: "24px",
            padding: "12px 24px",
          }}
        >
          {props.navigateIcon}
          {props.navigateText}
        </Button>
      </div>
      <div className="navbar-center">
        <h1>{props.title}</h1>
      </div>
      <div className="navbar-right">
        <div style={{ display: "flex", flexDirection: "column" }}>
          <span>{date}</span>
          <span>{time}</span>
        </div>
      </div>
    </div>
  );
}

export default Navbar;
