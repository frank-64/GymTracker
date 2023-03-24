export function getColorAndText(num) {
    let color, text;
  
    if (num < 20) {
      color = 'green';
      text = 'Very Quiet';
    } else if (num < 40) {
      color = 'limegreen';
      text = 'Quiet';
    } else if (num < 60) {
      color = 'gold';
      text = 'Moderate';
    } else if (num < 80) {
      color = 'tomato';
      text = 'Busy';
    } else {
      color = 'firebrick';
      text = 'Very Busy';
    }
  
    return { color, text };
}