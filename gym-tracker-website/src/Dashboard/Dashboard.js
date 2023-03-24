import "./Dashboard.css";
import ReactSpeedometer from "react-d3-speedometer";
import { useState, useEffect } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChartLine } from "@fortawesome/free-solid-svg-icons";
import { Col, Container, Row } from "react-bootstrap";
import Navbar from "../Components/Navbar";

function Dashboard() {
  const [occupancyLevel, setOccupancy] = useState(0);
  const [gymStatus, setGymStatus] = useState(false);
  const [gymStatusText, setGymStatusText] = useState("OPEN");
  const gymStatusStyle = {
    color: gymStatus ? "green" : "red",
    fontSize: "40px",
  };

  function determineGymStatus() {
    //Make call to backend to determine gym details

    if (!gymStatus) {
      setGymStatusText("CLOSED");
    }
  }

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
          console.log(occupancyObject);
          setOccupancy(occupancyObject.Percentage);
        });
      }
    });
  }

  useEffect(() => {
    //fetchData();
    determineGymStatus();
    const interval = setInterval(() => {
      //fetchData();
    }, 5000);

    return () => clearInterval(interval);
  });

  return (
    <div className="dashboard">
      <Navbar
        title="Gym Occupancy Tracker: Dashboard"
        navigateText="Gym Insights"
        navigateIcon={
          <FontAwesomeIcon icon={faChartLine} style={{ marginRight: "10px" }} />
        }
        navigateTarget="/insights/"
      />
      <Container>
        <Row>
          <Col md={6}>
            <h1>Second Column</h1>
          </Col>
          <Col md={6}>
            <header className="Dashboard-header">
              <h3>
                The gym is currently:{" "}
                <p style={gymStatusStyle}>{gymStatusText}</p>
              </h3>
              {gymStatus ? (
                <ReactSpeedometer
                  segments={5}
                  width={600}
                  height={450}
                  maxValue={100}
                  value={occupancyLevel}
                  segmentColors={[
                    "green",
                    "limegreen",
                    "gold",
                    "tomato",
                    "firebrick",
                  ]}
                  currentValueText={`${occupancyLevel}%`}
                  customSegmentLabels={[
                    {
                      text: "Very Very Quiet",
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
              ) : (
                <div className="closedGym">
                  {/* TODO: SET NEXT OPEN TIME */}
                  <h2>
                    Reopen at: <p>{"6:30am"}</p>
                  </h2>
                  <br></br>
                  <p>Please see opening hours for further details.</p>
                </div>
              )}
            </header>
          </Col>
        </Row>
      </Container>
    </div>
  );
}

export default Dashboard;
