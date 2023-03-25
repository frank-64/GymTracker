import { useState, useEffect } from "react";
import "./Admin.css";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDumbbell } from "@fortawesome/free-solid-svg-icons";
import Navbar from "../Components/Navbar";
import { Col, Container, Row, Table, Badge, Dropdown, Button } from "react-bootstrap";

function Admin() {
  const [gymDetails, setGymDetails] = useState("");
  const [showCustomPanel, setShowCustomPanel] = useState(false);

  const handleSelect = (eventKey) => {
    const updatedGymDetails = { ...gymDetails };
    if (eventKey === "opened") {
      updatedGymDetails.AdminClosedGym = false;
      updatedGymDetails.IsOpen = true;
    } else {
      updatedGymDetails.AdminClosedGym = true;
      updatedGymDetails.IsOpen = false;
    }
    setGymDetails(updatedGymDetails);
    postGymDetails(updatedGymDetails);
  };

  const togglePanel = () => {
    setShowCustomPanel(!showCustomPanel);
  };

  var headers = {
    Accept: "application/json",
    "Content-Type": "application/json",
  };

  function postGymDetails(updatedGymDetails) {
    fetch("https://gym-tracker-functions.azurewebsites.net/api/updateGymDetails?", {
      mode: "cors",
      method: "POST",
      headers: headers,
      body: JSON.stringify(updatedGymDetails),
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        return response.json();
      })
      .then((data) => {
        console.log(data);
      })
      .catch((error) => {
        console.error("There was a problem with the request:", error);
      });
  }

  useEffect(() => {
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
          });
        }
      });
    }

    fetchGymDetails();
  }, []);

  return (
    <div className="admin">
      <Navbar
        title="Gym Occupancy Tracker: Admin"
        navigateText="Gym Dashboard"
        navigateIcon={
          <FontAwesomeIcon icon={faDumbbell} style={{ marginRight: "10px" }} />
        }
        navigateTarget="/insights"
      />
      <Container fluid>
        <Row className="subtitle-row">
          <Col md={12} style={{ marginTop: "50px" }}>
            <div className="subtitle">
              <p>
                The Gym is currently:{" "}
                <Badge bg={gymDetails.IsOpen ? "success" : "danger"}>
                  {gymDetails.IsOpen ? "OPEN" : "CLOSED"}
                </Badge>
              </p>
            </div>
          </Col>
        </Row>
        <Row style={{ marginTop: "-100px" }}>
          <Col md={6} className="admin-column-left">
            <div className="admin-section">
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
              <div className="right-panel-toggle">
                <Button onClick={togglePanel} variant="outline-light">{showCustomPanel ? 'Update Standard Opening Hours' : 'Add Custom Opening Hour'}</Button>
              </div>
              <br/>
              <div className="gymstatus-dropdown">
              <p style={{ display: "inline-block", marginRight: "10px" }}>Set gym opening status:</p>
                <Dropdown onSelect={handleSelect} style={{ display: "inline-block" }}>
                  <Dropdown.Toggle variant={gymDetails.IsOpen ? "success" : "danger"} id="dropdown-basic">
                    {gymDetails.IsOpen ? "Open" : "Closed"}
                  </Dropdown.Toggle>

                  <Dropdown.Menu>
                    <Dropdown.Item eventKey="opened">Open</Dropdown.Item>
                    <Dropdown.Item eventKey="closed">Closed</Dropdown.Item>
                  </Dropdown.Menu>
                </Dropdown>
              </div>
            </div>
          </Col>
          <Col md={6} className="admin-column-right">
            <div className="admin-section">
              {showCustomPanel ? (
                <div className="custom-opening-hour">
                  <p>
                    Add Custom Opening Hours
                  </p>
                </div>
              ) : (
                <div className="standard-opening-hours">
                  <p>
                    Update Standard Opening Hours
                  </p>
                </div>
              )}
            </div>
          </Col>
        </Row>
      </Container>
    </div>
  );
}

export default Admin;
