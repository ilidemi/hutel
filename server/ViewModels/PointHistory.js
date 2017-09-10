import React from 'react';
import PropTypes from 'prop-types';

import moment from 'moment';

import Divider from 'material-ui/Divider';
import {List, ListItem} from 'material-ui/List';
import Subheader from 'material-ui/Subheader';

class PointHistory extends React.Component {
  constructor(props) {
    super(props);
    this.hiddenFields = ['id', 'tagId', 'date', 'submitTimestamp'];
  }
  
  humanizeDate(value) {
    var date = moment(value);
    var age = moment.duration(moment().diff(date));
    if (age.days() < 0) {
      return "In the future";
    } else if (age.days() == 0) {
      return "Today";
    } else if (age.days() == 1) {
      return "Yesterday";
    } else {
      return value + ", " + date.format('ddd') + ", " + age.days() + " days ago";
    }
  }

  render() {
    const style = {
      padding: 10,
      display: "flex",
      flexGrow: 1,
      flexDirection: "column",
      background: this.props.theme.historyBackground
    };
    
    if (this.props.points.length === 0) {
      return (
        <div style={style}>
          <h2>History is empty</h2>
        </div>
      );
    } else {
      const dateStyle = {
        color: this.props.theme.historyDateText
      };
      const listItemStyle = {
        fontWeight: 500
      };
      var pointsByDate = this.props.points.reduce((acc, point) => {
        (acc[point.date] = acc[point.date] || []).push(point);
        return acc;
      }, {});
      var historyItems = Object.entries(pointsByDate)
        .map(([date, points]) => {
          var dateItems = points.map(point => {
            var fieldTexts = Object.keys(point)
              .filter(key => !this.hiddenFields.includes(key))
              .map(key => `${key} ${point[key]}`);
            var secondaryText = fieldTexts.join("  ");
            return (
              <ListItem
                key={point.id}
                primaryText={point.tagId.toUpperCase()}
                secondaryText={secondaryText}
                disabled={true}
                style={listItemStyle}
              />
            );
          });
          return [
            <Subheader key={date} style={dateStyle}>{this.humanizeDate(date)}</Subheader>,
            ...dateItems,
            <Divider key={date + '-divider'} />
          ];
        });
      return (
        <div style={style}>
          <List>
            {historyItems}
          </List>
        </div>
      );
    }
  }
}

PointHistory.propTypes = {
  points: PropTypes.array.isRequired,
  theme: PropTypes.object
};

export default PointHistory;