import React, { useState } from "react";
import "./Navbar.css";

function Navbar(props) {
  const [time, setTime] = useState(new Date().toLocaleTimeString());

  // Update the time every second
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
      <div className="navbar-right">{time}</div>
    </div>
  );
}

export default Navbar;
