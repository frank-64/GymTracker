import "./App.css";
import ReactSpeedometer from "react-d3-speedometer";
import { useState, useEffect } from "react";

function App() {
  const [occupancyLevel, setOccupancy] = useState(0);
  var headers = {
    Accept: "application/json",
    "Content-Type": "application/json",
  };

  function fetchData() {
    fetch(
      "https://gym-tracker-functions.azurewebsites.net/api/determineGymOccupancy?",
      {
        mode: "cors",
        method: "GET",
        headers: headers,
      }
    ).then((response) => {
      if (response.ok) {
        response.json().then((json) => {
          var occupancyObject = JSON.parse(json);
          setOccupancy(occupancyObject.Value);
        });
      }
    });
  }

  useEffect(() => {
    fetchData();
    const interval = setInterval(() => {
      fetchData();
    }, 5000);

    return () => clearInterval(interval);
  });

  return (
    <div className="App">
      <script crossorigin src="..."></script>
      <header className="App-header">
        <h1 className="title">Gym Tracker Application</h1>
        <p>The gym is currently:</p>
        <div className="busyMeter">
          <ReactSpeedometer
            segments={5}
            width={800}
            height={700}
            maxValue={100}
            value={occupancyLevel}
            segmentColors={[
              "green",
              "limegreen",
              "gold",
              "tomato",
              "firebrick",
            ]}
            //currentValueText="Happiness Level"
            customSegmentLabels={[
              {
                text: "Very Quiet",
                position: "OUTSIDE",
                color: "#ffffff",
              },
              {
                text: "Quiet",
                position: "OUTSIDE",
                color: "#ffffff",
              },
              {
                text: "Moderate",
                position: "OUTSIDE",
                color: "#ffffff",
              },
              {
                text: "Busy",
                position: "OUTSIDE",
                color: "#ffffff",
              },
              {
                text: "Very Busy",
                position: "OUTSIDE",
                color: "#ffffff",
              },
            ]}
          />
        </div>
      </header>
    </div>
  );
}

export default App;
