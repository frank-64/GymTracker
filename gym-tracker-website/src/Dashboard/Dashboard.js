import "./Dashboard.css";
import ReactSpeedometer from "react-d3-speedometer";
import { useState, useEffect } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChartLine } from "@fortawesome/free-solid-svg-icons";
import { Col, Container, Row, Table, Badge } from "react-bootstrap";
import { getColorAndText } from "../Helper/helper";
import Navbar from "../Components/Navbar";
import { fetchData } from "../Helper/helper";

function Dashboard() {
  const [gymDetails, setGymDetails] = useState("");
  const [gymStatus, setGymStatus] = useState("");
  const [isOpen, setIsOpen] = useState(true);
  const [occupancy, setOccupancy] = useState("0");
  const getGymDetailsUrl = "https://gym-tracker-functions.azurewebsites.net/api/getGymDetails?";
  const getGymStatusUrl = "https://gym-tracker-functions.azurewebsites.net/api/getGymStatus?";

  const [gymOccupancyConfiguration, setGymOccupancyConfiguration] = useState({
    color: "",
    text: "",
  });

  const handleGymDetailsResponse = (gymDetailsObject) => {
    setGymDetails(gymDetailsObject)
  }

  const handleGymStatusResponse = (gymStatusObject) => {
    setGymStatus(gymStatusObject);
    setIsOpen(gymStatusObject.IsOpen);
    setOccupancy(gymStatusObject.CapacityPercentage);
    setGymOccupancyConfiguration(
      getColorAndText(gymStatusObject.CapacityPercentage)
    );
  }

  const handleGymStatusFetchNotOk = () => {
    //TODO: Add alerts to dashboard
  }

  const handleGymDetailsFetchNotOk = () => {
    //TODO: Add alerts to dashboard
  }

  const handleError = () => {
    //TODO: Add alerts to dashboard
  }

  useEffect(() => {
    function fetchGymDetails(){
      fetchData(getGymDetailsUrl, handleGymDetailsResponse, handleGymDetailsFetchNotOk, handleError);
    }


    function fetchGymStatus() {
      fetchData(getGymStatusUrl, handleGymStatusResponse, handleGymStatusFetchNotOk, handleError);
    }

    fetchGymDetails();
    fetchGymStatus();
  }, []);

  return (
    <div className="dashboard">
      <Navbar
        title="Gym Occupancy Tracker: Dashboard"
        navigateText="Gym Insights"
        navigateIcon={
          <FontAwesomeIcon icon={faChartLine} style={{ marginRight: "10px" }} />
        }
        navigateTarget="/insights"
      />
      <Container fluid>
        <Row className="subtitle-row">
          <Col md={12} style={{ marginTop: "50px" }}>
            <div className="subtitle">
              <p>
                The Gym is currently:{" "}
                <Badge bg={isOpen ? "success" : "danger"}>
                  {isOpen ? "OPEN" : "CLOSED"}
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
                  Opening hours for{" "}
                  <Badge>{gymDetails.GymName ? gymDetails.GymName : ""}</Badge>
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
                    {gymDetails.OpeningHours?.map((day) => (
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
              {isOpen ? (
                <div>
                  <ReactSpeedometer
                    segments={5}
                    width={600}
                    height={450}
                    maxValue={100}
                    value={gymStatus.CapacityPercentage}
                    segmentColors={[
                      "green",
                      "limegreen",
                      "gold",
                      "tomato",
                      "firebrick",
                    ]}
                    currentValueText={gymStatus.CapacityPercentage && (`${gymStatus.CapacityPercentage}%`)}
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
                        fontSize: "100%",
                      }}
                    >
                      {" "}
                      {gymOccupancyConfiguration.text}{" "}
                    </span>
                    <br />
                    <em>{`${occupancy}% capacity`}</em>
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
