import React from 'react';
import PropTypes from 'prop-types';

class PointHistory extends React.Component {
  constructor(props) {
    super(props);
    this.hiddenFields = ['id', 'tagId', 'date'];
  }

  render() {
    var pointRows = this.props.points.map(point => {
      var displayFields = Object.keys(point)
        .filter(key => !this.hiddenFields.includes(key))
        .map(key => <td>{point[key]}</td>);
      return (
        <tr>
          <td>{point.date}</td>
          <td>{point.tagId}</td>
          {displayFields}
        </tr>
      );
    });
    return (
      <table>
        <tbody>
          {pointRows}
        </tbody>
      </table>
    );
  }
}

PointHistory.propTypes = {
  points: PropTypes.array.isRequired
}

export default PointHistory;