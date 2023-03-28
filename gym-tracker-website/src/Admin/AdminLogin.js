import { useState } from "react";
import "./AdminLogin.css";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDumbbell } from "@fortawesome/free-solid-svg-icons";
import Navbar from "../Components/Navbar";
import { useNavigate } from "react-router-dom";

import {
  Col,
  Container,
  Row,
  Button,
  Form,
  Card,
  Spinner,
  Alert,
} from "react-bootstrap";

function AdminLogin() {
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  const handleUsernameChange = (e) => {
    setUsername(e.target.value);
  };

  const handlePasswordChange = (e) => {
    setPassword(e.target.value);
  };
  var headers = {
    Accept: "application/json",
    "Content-Type": "application/json",
  };

  const handleFormSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccess(null);
    try {
      const response = await fetch(
        "https://gym-tracker-functions.azurewebsites.net/api/determineAdminLogin?",
        {
          mode: "cors",
          method: "POST",
          headers: headers,
          body: JSON.stringify({ Username: username, Password: password }),
        }
      );
      if (response.ok) {
        setError(null);
        setSuccess("Valid username and password combination.");
        response.json().then((json) => {
          const jsonData = JSON.parse(json);
          const token = jsonData.token;
          localStorage.setItem("basicAuthToken", token);
          
          setTimeout(function() {
            navigate("/admin");
            setLoading(false);
          }, 2000);
        });
      } else {
        setError("Invalid username or password.");
        setSuccess(null);
        setLoading(false);
      }
    } catch (err) {
      setError("An unexpected error occurred.");
      setSuccess(null);
      setLoading(false);
    }
  };

  return (
    <div className="admin-login">
      <Navbar
        title="Gym Occupancy Tracker: Admin Login"
        navigateText="Gym Dashboard"
        navigateIcon={
          <FontAwesomeIcon icon={faDumbbell} style={{ marginRight: "10px" }} />
        }
        navigateTarget="/"
      />
      <Container fluid className="admin-login-container">
        <Row>
          <Col md={12} className="admin-login-column">
            <div className="admin-login-section">
              <Card className="login-card">
                <Card.Title>Admin Login</Card.Title>
                <Card.Body>
                  {error && <Alert variant="danger">{error}</Alert>}
                  {success && !error && (
                    <Alert variant="success">{success}</Alert>
                  )}
                  <Form onSubmit={handleFormSubmit}>
                    <Form.Group controlId="form-username">
                      <Form.Label>Username</Form.Label>
                      <Form.Control
                        type="text"
                        placeholder="Enter username"
                        value={username}
                        onChange={handleUsernameChange}
                        required
                      />
                    </Form.Group>

                    <Form.Group controlId="form-password">
                      <Form.Label>Password</Form.Label>
                      <Form.Control
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={handlePasswordChange}
                        required
                      />
                    </Form.Group>
                    <div className="form-button">
                      {loading ? (
                        <Spinner animation="border" variant="primary" />
                      ) : (
                        <Button variant="primary" type="submit">
                          Login
                        </Button>
                      )}
                    </div>
                  </Form>
                </Card.Body>
              </Card>
            </div>
          </Col>
        </Row>
      </Container>
    </div>
  );
}

export default AdminLogin;
