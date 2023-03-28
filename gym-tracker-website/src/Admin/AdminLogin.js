import { useState, useEffect, Fragment } from "react";
import "./AdminLogin.css";
import "./Admin.css";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDumbbell } from "@fortawesome/free-solid-svg-icons";
import Navbar from "../Components/Navbar";
import { useNavigate } from "react-router-dom";

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
  Card,
} from "react-bootstrap";

function AdminLogin() {
  const navigate = useNavigate();
  var bcrypt = require('bcryptjs');
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [hashedPassword, setHashedPassword] = useState("");

  const handleEmailChange = (e) => {
    setEmail(e.target.value);
  };

  const handlePasswordChange = (e) => {
    setPassword(e.target.value);
  };
  var headers = {
    Accept: "application/json",
    "Content-Type": "application/json",
  };

  const hashPass = () => {
    var salt = bcrypt.genSaltSync(10);
    var hash = bcrypt.hashSync(password, salt)
    setHashedPassword(hash);
  }

  // Compare hashed and unhashed
  //bcrypt.compareSync(password, hashedPassword)

  const handleFormSubmit = async (e) => {
    e.preventDefault();

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      alert('Please enter a valid email address');
      return;
    }

    hashPass();
    // try {
    //   const response = await fetch("/api/login", {
    //     mode: "cors",
    //     method: "POST",
    //     headers: headers,
    //     body: JSON.stringify({ email, hashedPassword }),
    //   });
    //   if (response.ok) {
    //     const { token } = await response.json();
    //     localStorage.setItem("authToken", token);
    //     navigate("/2fa");
    //   } else {
    //     alert("Invalid username or password.");
    //   }
    // } catch (err) {
    //   console.error(err);
    //   alert("An unexpected error occurred.");
    // }
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
                <Card.Title>
                  Admin Login
                </Card.Title>
                <Card.Body>
                  <Form onSubmit={handleFormSubmit}>
                    <Form.Group controlId="form-email">
                      <Form.Label>Email address</Form.Label>
                      <Form.Control
                        type="email"
                        placeholder="Enter email"
                        value={email}
                        onChange={handleEmailChange}
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
                      <Button variant="primary" type="submit">
                        Login
                      </Button>
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
