import { useState, useEffect, Fragment } from "react";
import "./Admin.css";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faHome } from "@fortawesome/free-solid-svg-icons";
import Navbar from "../Components/Navbar";
import {
  Col,
  Container,
  Row,
  Table,
  Badge,
  Dropdown,
  Button,
  Card,
  Form,
  Alert
} from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import jwtDecode from "jwt-decode";
import { fetchData, postData } from "../Helper/helper";

function Admin() {
  const navigate = useNavigate();
  const [gymDetails, setGymDetails] = useState("");
  const [gymStatus, setGymStatus] = useState("");
  const [updatingOpeningHours, setUpdatingOpeningHours] = useState(false);

  // Form variables for setting and getting
  const [isGymOpenInput, setIsGymClosedInput] = useState(false);
  const [loggedIn, setLoggedIn] = useState(false);
  const [dateInput, setDateInput] = useState("");
  const [startTimeInput, setStartTimeInput] = useState("");
  const [endTimeInput, setEndTimeInput] = useState("");
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  // API URLs
  const getGymDetailsUrl = "https://gym-tracker-functions.azurewebsites.net/api/getGymDetails?";
  const getGymStatusUrl = "https://gym-tracker-functions.azurewebsites.net/api/getGymStatus?";
  const postGymDetailsUrl = "https://gym-tracker-functions.azurewebsites.net/api/updateGymDetails?";
  const postGymStatusUrl = "https://gym-tracker-functions.azurewebsites.net/api/updateGymStatus?";
  const postCustomGymOpeningPeriodURL = "https://gym-tracker-functions.azurewebsites.net/api/setCustomOpeningPeriod?";

  const handleStartTimeInputChange = (e) => {
    setStartTimeInput(e.target.value);
  };

  const handleEndTimeInputChange = (e) => {
    setEndTimeInput(e.target.value);
  };

  const handleDateInputChange = (e) => {
    setDateInput(e.target.value);
  };

  const handleCheckboxChange = (event) => {
    setIsGymClosedInput(event.target.checked);
    if(isGymOpenInput){
      setStartTimeInput("");
      setEndTimeInput("");
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // Form validation
    if(dateInput === ""){
      setError("You must enter a date!");
    }else{
      if(isGymOpenInput){
        // Updated opening hours on a specific day
        // Need to ensure the values are set and the end time is not before the start time
        if((startTimeInput !== "" && endTimeInput !== "")){
          if(endTimeInput < startTimeInput){
            setError("End time must not be before start time.");
          }else{
            setError(null);
            postCustomGymOpeningPeriod();
          }
        }else{
          setError("Opening times not specified.");
        }
      }else{ // Closure on a specific date so no need for start/end time validation
        setError(null);
        setSuccess("Your update has been made.");
      }
    }
  }

  const handleSelect = (eventKey) => {
    const updatedGymStatus = { ...gymStatus };
    if (eventKey === "opened") {
      updatedGymStatus.AdminClosedGym = false;
      updatedGymStatus.IsOpen = true;
    } else {
      updatedGymStatus.AdminClosedGym = true;
      updatedGymStatus.IsOpen = false;
    }
    setGymStatus(updatedGymStatus);
    postGymStatus(updatedGymStatus);
  };

  function handleUpdateToggle() {
    setUpdatingOpeningHours((prev) => !prev);
  }

  function submitOpeningHours() {
    setUpdatingOpeningHours(false);
    postGymDetails(gymDetails);
  }

  const handleInputChange = (e) => {
    const { name, value } = e.target;

    const timeRegex = /^(0?[1-9]|1[0-2]):([0-5][0-9])\s?(AM|PM)$/i;
    if (!timeRegex.test(value)) {
      alert("The input did not match the expected pattern e.g. 9:30 PM");
      e.target.value = "";
      return;
    }

    const splitName = name.split("-");
    const isStartTime = splitName[0] === "StartTime" ? true : false;
    const updatedGymDetails = { ...gymDetails };
    const upperValue = value.toUpperCase();
    const updatedOpeningHours = updatedGymDetails.OpeningHours.map((day) => {
      if (day.DayOfWeek === splitName[1]) {
        if (isStartTime) {
          return {
            ...day,
            StartTime: upperValue,
          };
        } else {
          return {
            ...day,
            EndTime: upperValue,
          };
        }
      }
      return day;
    });
    updatedGymDetails.OpeningHours = updatedOpeningHours;
    setGymDetails(updatedGymDetails);
  };
  

  const handleGymDetailsResponse = (response) => {
    setGymDetails(response);
  }

  const handleGymStatusResponse = (response) => {
    setGymStatus(response);
  }

  const handleCustomOpeningPostNotOk = () => {
    setError("An issue occurred when adding the closure or setting a specific opening hour.");
  }

  const handleGymDetailsPostNotOk = () => {

  }

  const handleGymStatusPostNotOk = () => {

  }

  const handleError = (error) => {
    console.error(error);
  }

  const handleOk = (json) => {

  }

  const handleCustomOpeningOk = (json) => {
    setSuccess("Your update has been made successfully.");
  }

  const handleNotOk = () => {

  }

  function postCustomGymOpeningPeriod(customOpeningPeriod) {
    var body = JSON.stringify(customOpeningPeriod);
    postData(postGymDetailsUrl, body, handleCustomOpeningOk, handleCustomOpeningPostNotOk, handleError);
  }


  function postGymDetails(updatedGymDetails) {
    var body = JSON.stringify(updatedGymDetails);
    postData(postGymDetailsUrl, body, handleOk, handleGymDetailsPostNotOk, handleError);
  }

  function postGymStatus(updatedGymStatus) {
    var body = JSON.stringify(updatedGymStatus);
    postData(postGymStatusUrl, body, handleOk, handleGymStatusPostNotOk, handleError);
  }

  useEffect(() => {
    function fetchGymDetails() {
      fetchData(getGymDetailsUrl, handleGymDetailsResponse, handleNotOk, handleError);
    }

    function fetchGymStatus() {
      fetchData(getGymStatusUrl, handleGymStatusResponse, handleNotOk, handleError);
    }

  fetchGymStatus();
  fetchGymDetails();
  }, []);

  // Redirect the user back to the login page if they no longer have a token or it has expired
  useEffect(() => {
    const token = localStorage.getItem("authToken");
    if (!token || tokenExpired(token)) {
      setLoggedIn(false);
      navigate("/admin-login");
    } else {
      setLoggedIn(true);
    }
  }, [navigate]);

  const tokenExpired = (token) => {
    const decodedToken = jwtDecode(token);
    const currentDateTime = new Date();
    if (decodedToken.exp * 1000 < currentDateTime.getTime()) {
      return true; // Token not expired yet
    }
    return false; // Expired token
  };

  return (
    <div className="admin">
      <Navbar
        title="Gym Occupancy Tracker: Admin"
        navigateText="Logout"
        navigateIcon={
          <FontAwesomeIcon icon={faHome} style={{ marginRight: "10px" }} />
        }
        navigateTarget="/admin-login"
        logout={true}
      />
      {loggedIn && (
        <Container fluid>
          <Row className="subtitle-row">
            <Col md={12} style={{ marginTop: "50px" }}>
              <div className="subtitle">
                <p>
                  The Gym is currently:{" "}
                  <Badge bg={gymStatus.IsOpen ? "success" : "danger"}>
                    {gymStatus.IsOpen ? "OPEN" : "CLOSED"}
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
                    <Badge>
                      {gymDetails.GymName ? gymDetails.GymName : ""}
                    </Badge>
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
                        {gymDetails.OpeningHours?.map((day) => (
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
                        {gymDetails.OpeningHours?.map((day) => (
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
                      style={{
                        display: "flex",
                        justifyContent: "space-between",
                      }}
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
                      variant={gymStatus.IsOpen ? "success" : "danger"}
                      id="dropdown-basic"
                    >
                      {gymStatus.IsOpen ? "Open" : "Closed"}
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
                    <Card className="custom-opening-hour-card">
                      <Card.Title>Add Closure or Specific Opening Hours</Card.Title>
                        <Card.Body>
                          {error && <Alert variant="danger">{error}</Alert>}
                          {success && !error && (
                            <Alert variant="success">{success}</Alert>
                          )}
                          <Form onSubmit={handleSubmit}>
                            <Form.Group style={{ marginBottom: "20px" }}>
                              <Form.Label>Date:</Form.Label>
                              <Form.Control 
                                type="date"
                                value={dateInput}
                                onChange={handleDateInputChange}
                                required
                              />
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
                                  <Form.Label>Start time:</Form.Label>
                                  <Form.Control
                                    value={startTimeInput}
                                    onChange={handleStartTimeInputChange}
                                    type="time"
                                  />
                                </Form.Group>
                                <Form.Group style={{ marginBottom: "25px" }}>
                                  <Form.Label>End time:</Form.Label>
                                  <Form.Control
                                    value={endTimeInput}
                                    onChange={handleEndTimeInputChange}
                                    type="time"
                                  />
                                </Form.Group>
                              </fieldset>
                            </Fragment>
                            <div className="form-button">
                              <Button variant="success" type="submit">Add Closure/Opening Hour Update</Button>
                            </div>
                          </Form>
                        </Card.Body>
                      </Card>
                  </div>
                </div>
              </div>
            </Col>
          </Row>
        </Container>
      )}
    </div>
  );
}

export default Admin;
