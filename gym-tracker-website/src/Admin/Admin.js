import { useState, useEffect, Fragment, navigate } from "react";
import "./Admin.css";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDumbbell } from "@fortawesome/free-solid-svg-icons";
import Navbar from "../Components/Navbar";
import {
  Col,
  Container,
  Row,
  Table,
  Badge,
  Dropdown,
  Button,
  Alert,
  Form,
} from "react-bootstrap";

function Admin() {
  const [gymDetails, setGymDetails] = useState("");
  const [tempGymDetails, setTempGymDetails] = useState("");
  const [isGymOpenInput, setIsGymClosedInput] = useState(false);
  const [updatingOpeningHours, setUpdatingOpeningHours] = useState(false);
  const [alerts, setAlerts] = useState([]);

  const handleCheckboxChange = (event) => {
    setIsGymClosedInput(event.target.checked);
  };

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

  function handleUpdateToggle() {
    setUpdatingOpeningHours((prev) => !prev);
  }

  function submitOpeningHours() {
    setUpdatingOpeningHours(false);
    if (tempGymDetails === "") {
      addAlert("Error:", "You did not make any changes to update!", "danger");
    } else {
      postGymDetails(tempGymDetails);
      setGymDetails(tempGymDetails);
      setTempGymDetails(gymDetails);
    }
  }

  const addAlert = (messageTitle, message, alertType) => {
    setAlerts((prevAlerts) => [
      ...prevAlerts,
      {
        messageTitle,
        message,
        alertType,
      },
    ]);
  };

  const removeAlert = (id) => {
    setAlerts((prevAlerts) =>
      prevAlerts.filter((alert) => alert.messageTitle !== id)
    );
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;

    const timeRegex = /^(0?[1-9]|1[0-2]):([0-5][0-9])\s?(AM|PM)$/i;
    if(!timeRegex.test(value)){
      alert("The input did not match the expected pattern e.g. 9:30 PM");
      e.target.value = "";
      return;
    }

    const splitName = name.split("-");
    const isStartTime = splitName[0] === "StartTime" ? true : false;
    const updatedGymDetails = { ...gymDetails };
    const updateHours = updatedGymDetails.Hours.map((hour) => {
      if (hour.DayOfWeek === splitName[1]) {
        if (isStartTime) {
          return {
            ...hour,
            StartTime: value,
          };
        } else {
          return {
            ...hour,
            EndTime: value,
          };
        }
      }
      return hour;
    });
    updatedGymDetails.Hours = updateHours;
    setTempGymDetails(updatedGymDetails);
  };

  var headers = {
    Accept: "application/json",
    "Content-Type": "application/json",
  };

  function postGymDetails(updatedGymDetails) {
    fetch(
      "https://gym-tracker-functions.azurewebsites.net/api/updateGymDetails?",
      {
        mode: "cors",
        method: "POST",
        headers: headers,
        body: JSON.stringify(updatedGymDetails),
      }
    )
      .then((response) => {
        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        return response;
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
            console.log(gymDetailsObject);
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
        navigateTarget="/"
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
                  {updatingOpeningHours ? (
                    <tbody>
                      {gymDetails.Hours?.map((day) => (
                        <tr key={day.DayOfWeek}>
                          <td>{day.DayOfWeek}</td>
                          <td>
                            <input
                              type="text"
                              className="form-control"
                              name={`StartTime-${day.DayOfWeek}`}
                              placeholder={day.StartTime}
                              onBlur={handleInputChange}
                            />
                          </td>
                          <td>
                            <input
                              type="text"
                              className="form-control"
                              name={`EndTime-${day.DayOfWeek}`}
                              placeholder={day.EndTime}
                              onBlur={handleInputChange}
                            />
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  ) : (
                    <tbody>
                      {gymDetails.Hours?.map((day) => (
                        <tr key={day.DayOfWeek}>
                          <td>{day.DayOfWeek}</td>
                          <td>{day.StartTime}</td>
                          <td>{day.EndTime}</td>
                        </tr>
                      ))}
                    </tbody>
                  )}
                </Table>
              </div>
              <div className="right-panel-toggle">
                {updatingOpeningHours ? (
                  <div
                    style={{ display: "flex", justifyContent: "space-between" }}
                  >
                    <Button
                      variant="success"
                      style={{ marginRight: "10px" }}
                      onClick={submitOpeningHours}
                    >
                      Update
                    </Button>
                    <Button variant="danger" onClick={handleUpdateToggle}>
                      Cancel
                    </Button>
                  </div>
                ) : (
                  <Button variant="success" onClick={handleUpdateToggle}>
                    Update Standard Opening Hours
                  </Button>
                )}
              </div>
              <br />
              <div className="gymstatus-dropdown">
                <p style={{ display: "inline-block", marginRight: "10px" }}>
                  Set gym opening status:
                </p>
                <Dropdown
                  onSelect={handleSelect}
                  style={{ display: "inline-block" }}
                >
                  <Dropdown.Toggle
                    variant={gymDetails.IsOpen ? "success" : "danger"}
                    id="dropdown-basic"
                  >
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
              <div className="custom-opening-hour">
                <div>
                  <p>Add Closure or Set Specific Opening Hours</p>
                </div>
                <div>
                  <Form>
                    <Form.Group style={{ marginBottom: "20px" }}>
                      <Form.Label>Date:</Form.Label>
                      <Form.Control type="date" style={{ width: "50%" }} />
                    </Form.Group>
                    <Form.Group
                      style={{
                        display: "flex",
                        alignItems: "center",
                      }}
                    >
                      <Form.Label style={{ marginRight: "10px" }}>
                        Will the gym be open?:
                      </Form.Label>
                      <Form.Check
                        inline
                        type="checkbox"
                        checked={isGymOpenInput}
                        onChange={handleCheckboxChange}
                      />
                    </Form.Group>
                    <Fragment>
                      <fieldset disabled={!isGymOpenInput}>
                        <Form.Group style={{ marginBottom: "25px" }}>
                          <Form.Label>Start Time:</Form.Label>
                          <Form.Control type="time" style={{ width: "50%" }} />
                        </Form.Group>
                        <Form.Group style={{ marginBottom: "25px" }}>
                          <Form.Label>End Time:</Form.Label>
                          <Form.Control type="time" style={{ width: "50%" }} />
                        </Form.Group>
                      </fieldset>
                    </Fragment>
                  </Form>
                </div>
                <div>
                  <Button variant="success">
                    Add
                  </Button>
                </div>
              </div>
            </div>
          </Col>
        </Row>
        {/* TODO: Come back to this as errors are hard to see */}
        <Col md={12}>
          <div id="alertContainer" className="alert-container">
            {alerts.map((alert) => (
              <Alert
                key={alert.messageTitle}
                variant={alert.alertType}
                dismissible
                onClose={() => removeAlert(alert.messageTitle)}
                className="footer"
              >
                <strong>{alert.messageTitle}</strong>
                <span>{alert.message}</span>
              </Alert>
            ))}
          </div>
        </Col>
      </Container>
    </div>
  );
}

export default Admin;
