import "./App.css";
import ReactSpeedometer from "react-d3-speedometer";
import { useState, useEffect } from "react";

function App() {
  const [error, setError] = useState(null);
  const [isLoaded, setIsLoaded] = useState(false);
  const [occupancyLevel, setOccupancy] = useState(0);
  var headers = {
    Accept: "application/json",
    "Content-Type": "application/json",
  };

  useEffect(() => {
    fetch(
      "https://gym-tracker-functions.azurewebsites.net/api/determineGymOccupancy?",
      {
        method: "GET",
        mode: "no-cors",
        headers: headers,
      }
    )
      .then((res) => res.json())
      .then(
        (result) => {
          setIsLoaded(true);
          setOccupancy(result);
        },
        // Note: it's important to handle errors here
        // instead of a catch() block so that we don't swallow
        // exceptions from actual bugs in components.
        (error) => {
          setIsLoaded(true);
          setError(error);
        }
      );
  }, []);

  return (
    <div className="App">
      <script crossorigin src="..."></script>
      {console.log(occupancyLevel)}
      <header className="App-header">
        <h1 className="title">Gym Tracker Application</h1>
        <p>This gym is currently:</p>
        <div className="busyMeter">
          <ReactSpeedometer
            forceRender={true}
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
