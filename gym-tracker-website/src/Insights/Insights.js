import "./Insights.css";
import Navbar from "../Components/Navbar";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDumbbell } from "@fortawesome/free-solid-svg-icons";
import { useState, useEffect } from "react";
import { Col, Container, Row, Table, Badge } from "react-bootstrap";
import { getColorAndText } from "../Helper/helper";
import { fetchData } from "../Helper/helper";
import { BarChart, ResponsiveContainer, Bar, XAxis, YAxis, Legend } from "recharts";

function Insights() {
  const [isOpen, setIsOpen] = useState(true);
  const [dayOfWeek, setDayOfWeek] = useState("");
  const [dailyPeakOccupancyData, setDailyPeakOccupancyData] = useState("");
  const [hourlyPeakOccupancyData, setHourlyPeakOccupancyData] = useState("");

  const data = [
    {
      name: "Page A",
      uv: 4000,
    },
    {
      name: "Page B",
      uv: 3000,
    },
    {
      name: "Page C",
      uv: 2000,
    },
    {
      name: "Page D",
      uv: 2780,
    },
    {
      name: "Page E",
      uv: 1890,
    },
    {
      name: "Page F",
      uv: 2390,
    },
    {
      name: "Page G",
      uv: 3490,
    },
  ];

  const getGymStatusUrl =
    "https://gym-tracker-functions.azurewebsites.net/api/getGymStatus?";
  const getGymInsightsUrl =
    "https://gym-tracker-functions.azurewebsites.net/api/getGymInsights?";

  const handleGymStatusResponse = (gymStatusObject) => {
    setIsOpen(gymStatusObject.IsOpen);
  };

  const handleGymInsightsResponse = (gymInsightsObject) => {
    setDayOfWeek(gymInsightsObject.DayOfWeek);
    console.log(gymInsightsObject);

    setDailyPeakOccupancyData(gymInsightsObject.AverageDailyPeakOccupancy);
    setHourlyPeakOccupancyData(gymInsightsObject.AverageHourlyPeakOccupancy);
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
            <ResponsiveContainer width="100%" height="30%">
              <BarChart
                width={500}
                height={200}
                data={data}
                margin={{
                  top: 5,
                  right: 30,
                  left: 20,
                  bottom: 5,
                }}
              >
                <XAxis dataKey="name" />
                <YAxis />
                <Legend />
                <Bar dataKey="pv" fill="#8884d8" />
                <Bar dataKey="uv" fill="#82ca9d" />
              </BarChart>
            </ResponsiveContainer>
            <ResponsiveContainer width="100%" height="30%">
              <BarChart
                width={500}
                height={200}
                data={data}
                margin={{
                  top: 5,
                  right: 30,
                  left: 20,
                  bottom: 5,
                }}
              >
                <XAxis dataKey="name" />
                <YAxis />
                <Legend />
                <Bar dataKey="pv" fill="#8884d8" />
                <Bar dataKey="uv" fill="#82ca9d" />
              </BarChart>
            </ResponsiveContainer>
          </Col>
          <Col md={6}>
            <div className="insights-section"></div>
          </Col>
        </Row>
      </Container>
    </div>
  );
}

export default Insights;
