import React, { useState } from "react";
import "./Navbar.css";

function Navbar(props) {
  const options = {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric'
  };

  const [time, setTime] = useState(new Date().toLocaleTimeString());
  const [date, setDate] = useState(new Date().toLocaleDateString('en-GB', options));

  // Update the date and time
  setInterval(() => setTime(new Date().toLocaleTimeString()), 1000);

  return (
    <div className="navbar">
      <div className="navbar-left">
        <button
          onClick={() => {
            window.location.href = props.navigateTarget;
          }}
        >
          {props.navigateText}
        </button>
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
