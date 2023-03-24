import "./Insights.css";
import Navbar from "../Components/Navbar";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faDumbbell } from '@fortawesome/free-solid-svg-icons'

function Insights() {
  return (
    <div className="insights">
      <Navbar
        title="Gym Occupancy Tracker: Insights"
        navigateText="Gym Dashboard"
        navigateIcon={<FontAwesomeIcon icon={faDumbbell} style={{ marginRight: "10px" }}/>}
        navigateTarget="/"
      />
      <script crossorigin src="..."></script>
      <header className="Insights-header"></header>
    </div>
  );
}

export default Insights;
