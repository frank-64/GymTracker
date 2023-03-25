import "./Dashboard.css";
import ReactSpeedometer from "react-d3-speedometer";
import { useState, useEffect } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChartLine } from "@fortawesome/free-solid-svg-icons";
import { Col, Container, Row, Table, Badge } from "react-bootstrap";
import { getColorAndText } from "../Helper/helper";
import Navbar from "../Components/Navbar";

function Dashboard() {
  const [occupancyLevel, setOccupancy] = useState(0);
  const [gymDetails, setGymDetails] = useState('');
  const [gymStatus, setGymStatus] = useState(true);
  const [gymStatusText, setGymStatusText] = useState("OPEN");
  const [gymOccupancyConfiguration, setGymOccupancyConfiguration] = useState({
    color: "",
    text: "",
  });

  useEffect(() => {
    function determineGymStatus() {
      //TODO: Make call to backend to determine gym details
  
      if (!gymStatus) {
        setGymStatusText("CLOSED");
        // TODO: Move this VV
        // setGymStatus(false);
      }
    }
  
    var headers = {
      Accept: "application/json",
      "Content-Type": "application/json",
    };

    function fetchGymDetails() {
      fetch(
        "https://gym-tracker-functions.azurewebsites.net/api/getGymDetails?",
        {
          mode: "cors",
          method: "GET",
          headers: headers,
        }
      ).then((response) => {
        if (response.ok) {
          response.json().then((json) => {
            var gymDetailsObject = JSON.parse(json);
            setGymDetails(gymDetailsObject);
            determineGymStatus(gymDetailsObject.Hours);
          });
        }
      });
    }

    function fetchGymOccupancy() {
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
            setGymOccupancyConfiguration(
              getColorAndText(occupancyObject.Percentage)
            );
          });
        }
      });
    }

    fetchGymOccupancy();
    fetchGymDetails();
    determineGymStatus();
  }, []);

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
      <Container fluid>
        <Row className="subtitle-row">
          <Col md={12} style={{ marginTop: "50px" }}>
            <div className="subtitle">
              <p>
                The Gym is currently:{" "}
                <Badge bg={gymStatus ? "success" : "danger"}>
                  {gymStatusText}
                </Badge>
              </p>
            </div>
          </Col>
        </Row>
        <Row style={{ marginTop: "-100px" }}>
          <Col md={6} className="dashboard-column-left">
            <div className="dashboard-section">
              <div>
                <p>
                  Opening hours for <Badge>{gymDetails.GymName ? gymDetails.GymName : ''}</Badge>
                </p>
              </div>
              <div>
                <Table style={{ color: "white" }}>
                  <thead>
                    <tr>
                      <th>Day</th>
                      <th>Opening Time</th>
                      <th>Closing Time</th>
                    </tr>
                  </thead>
                  <tbody>
                  {gymDetails.Hours?.map((day) => (
                    <tr key={day.DayOfWeek}>
                      <td>{day.DayOfWeek}</td>
                      <td>{day.StartTime}</td>
                      <td>{day.EndTime}</td>
                    </tr>
                  ))}
                  </tbody>
                </Table>
              </div>
            </div>
          </Col>
          <Col md={6} className="dashboard-column-right">
            <div className="dashboard-section">
              {gymStatus ? (
                <div>
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
                  <p>
                    Gym occupancy status is{" "}
                    <span
                      style={{
                        color: gymOccupancyConfiguration.color,
                        display: "inline",
                        fontSize: "100%"
                      }}
                    >
                      {" "}
                      {gymOccupancyConfiguration.text}{" "}
                    </span>
                    <br />
                    <em>{`${occupancyLevel}% capacity`}</em>
                  </p>
                </div>
              ) : (
                <div className="closedGym">
                  {/* TODO: SET NEXT OPEN TIME */}
                  <h2>Reopen at:</h2>
                  <p>{"6:30am"}</p>
                  <br />
                  <p>Please see opening hours for further details.</p>
                </div>
              )}
            </div>
          </Col>
        </Row>
      </Container>
    </div>
  );
}

export default Dashboard;
