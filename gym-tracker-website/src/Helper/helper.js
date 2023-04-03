export function getColorAndText(num) {
  let color, text;

  if (num < 20) {
    color = "green";
    text = "Very Quiet";
  } else if (num < 40) {
    color = "limegreen";
    text = "Quiet";
  } else if (num < 60) {
    color = "gold";
    text = "Moderate";
  } else if (num < 80) {
    color = "tomato";
    text = "Busy";
  } else {
    color = "firebrick";
    text = "Very Busy";
  }

  return { color, text };
}

const headers = {
  Accept: "application/json",
  "Content-Type": "application/json",
};

export function fetchData(url, handleResponse, handleNotOk, handleError) {
  fetch(url, {
    mode: "cors",
    method: "GET",
    headers: headers,
  })
    .then((response) => {
      if (response.ok) {
        response.json().then((json) => {
          handleResponse(JSON.parse(json));
        });
      } else if (response.status === 429) {
        const retryAfter = response.headers.get("Retry-After");
        if (retryAfter) {
          setTimeout(() => {
            fetchData(url, handleResponse, handleNotOk, handleError);
          }, retryAfter * 1000);
        }
      } else {
        handleNotOk();
      }
    })
    .catch((error) => {
      handleError(error);
    });
}

export function postData(url, body, handleResponse, handleNotOk, handleError) {
  fetch(url, {
    mode: "cors",
    method: "POST",
    headers: headers,
    body: body,
  })
    .then((response) => {
      if (response.ok) {
        response.json().then((json) => {
          handleResponse(JSON.parse(json));
        });
      } else if (response.status === 429) {
        const retryAfter = response.headers.get("Retry-After");
        if (retryAfter) {
          setTimeout(() => {
            fetchData(url, body, handleResponse, handleNotOk, handleError);
          }, retryAfter * 1000);
        }
      } else {
        handleNotOk();
      }
    })
    .catch((error) => {
      handleError(error);
    });
}
