import "./Insights.css";
import Navbar from "../Components/Navbar";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDumbbell, faSquare } from "@fortawesome/free-solid-svg-icons";
import { useState, useEffect } from "react";
import { Col, Container, Row, Badge, Card } from "react-bootstrap";
import { getColorAndText } from "../Helper/helper";
import { fetchData } from "../Helper/helper";
import {
  BarChart,
  ResponsiveContainer,
  Bar,
  XAxis,
  YAxis,
  Label,
} from "recharts";

function Insights() {
  const [isOpen, setIsOpen] = useState(true);
  const [dayOfWeek, setDayOfWeek] = useState("");
  const [dailyPeakOccupancyData, setDailyPeakOccupancyData] = useState("");
  const [hourlyPeakOccupancyData, setHourlyPeakOccupancyData] = useState("");
  const [equipmentUsage, setEquipmentUsage] = useState("");
  const getGymStatusUrl =
    "https://gym-tracker-functions.azurewebsites.net/api/getGymStatus?";
  const getGymInsightsUrl =
    "https://gym-tracker-functions.azurewebsites.net/api/getGymInsights?";

  const handleGymStatusResponse = (gymStatusObject) => {
    setIsOpen(gymStatusObject.IsOpen);
  };

  const handleGymInsightsResponse = (gymInsightsObject) => {
    setDayOfWeek(gymInsightsObject.DayOfWeek);

    setDailyPeakOccupancyData(gymInsightsObject.AverageDailyPeakOccupancy);
    setHourlyPeakOccupancyData(gymInsightsObject.AverageHourlyPeakOccupancy);
    setEquipmentUsage(gymInsightsObject.EquipmentUsage);
  };

  const handleGymStatusFetchNotOk = () => {
    //TODO: Add alerts to dashboard
  };

  const handleError = () => {
    //TODO: Add alerts to dashboard
  };

  useEffect(() => {
    function fetchGymStatus() {
      fetchData(
        getGymStatusUrl,
        handleGymStatusResponse,
        handleGymStatusFetchNotOk,
        handleError
      );
    }

    function fetchGymInsights() {
      fetchData(
        getGymInsightsUrl,
        handleGymInsightsResponse,
        handleGymStatusFetchNotOk,
        handleError
      );
    }

    fetchGymInsights();
    fetchGymStatus();
  }, []);
  return (
    <div className="insights">
      <Navbar
        title="Gym Occupancy Tracker: Insights"
        navigateText="Gym Dashboard"
        navigateIcon={
          <FontAwesomeIcon icon={faDumbbell} style={{ marginRight: "10px" }} />
        }
        navigateTarget="/"
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
        <Row>
          <Col md={6} style={{ marginTop: "50px" }}>
            <ResponsiveContainer width="100%" height="35%">
              <BarChart
                width={500}
                height={200}
                data={dailyPeakOccupancyData}
                margin={{
                  top: 5,
                  right: 30,
                  left: 20,
                  bottom: 0,
                }}
              >
                <XAxis
                  dataKey="name"
                  tick={{ fill: "white", fontSize: "small" }}
                />
                <YAxis tick={{ fill: "white", fontSize: "large" }}>
                  <Label
                    value="Capacity %"
                    position="insideLeft"
                    angle={-90}
                    style={{
                      textAnchor: "middle",
                      fill: "#fff",
                      fontWeight: "bold",
                    }}
                  />
                </YAxis>
                <Bar dataKey="occupancy" fill="#8884d8" />
                <text
                  x="50%"
                  y="12"
                  textAnchor="middle"
                  fontWeight="bold"
                  fontSize={16}
                  fill="white"
                >
                  Daily peak occupancy
                </text>
              </BarChart>
            </ResponsiveContainer>
            <div style={{ height: "50px" }}></div>
            <ResponsiveContainer width="100%" height="35%">
              <BarChart
                width={500}
                height={200}
                data={hourlyPeakOccupancyData}
                margin={{
                  top: 5,
                  right: 30,
                  left: 20,
                  bottom: 0,
                }}
              >
                <XAxis dataKey="name" tick={{ fill: "white" }} interval={2} />
                <YAxis tick={{ fill: "white", fontSize: "medium" }}>
                  <Label
                    value="Capacity %"
                    position="insideLeft"
                    angle={-90}
                    style={{
                      textAnchor: "middle",
                      fill: "#fff",
                      fontWeight: "bold",
                    }}
                  />
                </YAxis>
                <Bar dataKey="occupancy" fill="#82ca9d" />
                <text
                  x="50%"
                  y="12"
                  textAnchor="middle"
                  fontWeight="bold"
                  fontSize={16}
                  fill="white"
                >
                  Hourly occupancy for {dayOfWeek}
                </text>
              </BarChart>
            </ResponsiveContainer>
          </Col>
          <Col md={6}>
            <div className="insights-section">
              <Row>
                <Col md={7} lg={7} xl={12}>
                  <h3>
                    Equipment Usage Estimates{" "}
                    <FontAwesomeIcon
                      icon={faDumbbell}
                      style={{ marginLeft: "10px" }}
                    />
                  </h3>
                  <ul className="equipment-usage-list">
                    {equipmentUsage &&
                      equipmentUsage.map((equipment) => (
                        <li
                          style={{
                            color: getColorAndText(equipment.UsagePercentage)
                              .color,
                          }}
                          key={equipment.Name}
                        >
                          {equipment.Name}: {equipment.UsagePercentage}%
                        </li>
                      ))}
                  </ul>
                </Col>
                <Col md={5} lg={5} xl={12} className="usage-column">
                  <Card className="key-card">
                    <Card.Title>Usage Key</Card.Title>
                    <Card.Body>
                      <ul className="usage-list">
                        <li key="vbusy">
                          <FontAwesomeIcon
                            icon={faSquare}
                            style={{
                              marginRight: "10px",
                              color: "firebrick",
                            }}
                          />
                          {"Very Busy (>80%)"}
                        </li>
                        <li key="busy">
                          <FontAwesomeIcon
                            icon={faSquare}
                            style={{ marginRight: "10px", color: "tomato" }}
                          />
                          {"Busy (60% - 80%)"}
                        </li>
                        <li key="moderate">
                          <FontAwesomeIcon
                            icon={faSquare}
                            style={{ marginRight: "10px", color: "gold" }}
                          />
                          {"Moderate (40% - 60%)"}
                        </li>
                        <li key="quiet">
                          <FontAwesomeIcon
                            icon={faSquare}
                            style={{
                              marginRight: "10px",
                              color: "limegreen",
                            }}
                          />
                          {"Quiet (20% - 40%)"}
                        </li>
                        <li key="vquiet">
                          <FontAwesomeIcon
                            icon={faSquare}
                            style={{ marginRight: "10px", color: "green" }}
                          />
                          {"Very Quiet (<20%)"}
                        </li>
                      </ul>
                    </Card.Body>
                  </Card>
                </Col>
              </Row>
            </div>
          </Col>
        </Row>
      </Container>
    </div>
  );
}

export default Insights;
