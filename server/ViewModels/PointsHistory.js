import React from 'react';
import PropTypes from 'prop-types';
import update from 'immutability-helper';

import $ from 'jquery';
import moment from 'moment';

import Divider from 'material-ui/Divider';
import FontIcon from 'material-ui/FontIcon';
import IconButton from 'material-ui/IconButton';
import { List, ListItem } from 'material-ui/List';
import Subheader from 'material-ui/Subheader';

class PointsHistory extends React.Component {
  constructor(props) {
    super(props);
    this.hiddenFields = ['id', 'tagId', 'date', 'submitTimestamp'];
    this.state = {};
  }

  componentWillUnmount() {
    for (var pointState of Object.values(this.state)) {
      clearTimeout(pointState.timer);
    }
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

  pointStateUpdater(pointId, spec) {
    return (prevState) => {
      var verb = prevState[pointId] ? "$merge" : "$set";
      return update(prevState, {
        [pointId]: {
          [verb]: spec
        }
      });
    };
  }

  deletePoint(pointId) {
    console.log("Deleting point", pointId);
    this.setState(this.pointStateUpdater(pointId, { loading: true }),
      () => {
        $.ajax({
          url: "/api/points/" + pointId,
          method: "DELETE",
          success: () => {
            this.setState(
              this.pointStateUpdater(pointId, { loading: false }),
              () => this.props.notifyPointsChanged());
          },
          error: (xhr, status, err) => {
            console.error(err);
            this.setState(this.pointStateUpdater(pointId, { loading: false }));
          }
        });
      });
  }

  showDeleteButton(pointId) {
    var pointState = this.state[pointId];
    if (pointState) {
      clearTimeout(pointState.timer);
    }

    var timer = setTimeout(() => {
      this.setState(this.pointStateUpdater(pointId, {
        deleteButtonVisible: false,
        timer: null
      }));
    }, 3000);

    this.setState(this.pointStateUpdater(pointId, {
      deleteButtonVisible: true,
      timer: timer
    }));
  }

  click() {
    console.log("click");
  }

  clickclick() {
    console.log("clickclick");
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
      var pointsByDate = this.props.points
        .filter(point => !this.props.sensitiveHidden || !this.props.tagsById[point.tagId].isSensitive)
        .reduce((acc, point) => {
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

            var pointState = this.state[point.id];
            var loading = pointState && pointState.loading;
            var deleteButtonVisible = pointState && pointState.deleteButtonVisible;

            const loadingIconStyle = {
              fontSize: 20
            };

            var loadingIcon = loading
              ? <FontIcon
                className="material-icons"
                style={loadingIconStyle}
              >
                hourglass_empty
              </FontIcon>
              : undefined;

            var deleteButton = !loading && deleteButtonVisible
              ? <IconButton
                iconClassName="material-icons"
                onClick={this.deletePoint.bind(this, point.id)}
              >
                delete
              </IconButton>
              : undefined;

            return (
              <ListItem
                key={point.id}
                primaryText={point.tagId.toUpperCase()}
                secondaryText={secondaryText}
                disabled={true}
                style={listItemStyle}
                onClick={this.showDeleteButton.bind(this, point.id)}
                rightIconButton={deleteButton}
                rightIcon={loadingIcon}
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

PointsHistory.propTypes = {
  points: PropTypes.array.isRequired,
  tagsById: PropTypes.object.isRequired,
  sensitiveHidden: PropTypes.bool,
  theme: PropTypes.object,
  notifyPointsChanged: PropTypes.func
};

export default PointsHistory;