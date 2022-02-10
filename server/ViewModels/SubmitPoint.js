import React from 'react';
import PropTypes from 'prop-types';
import update from 'immutability-helper';

import $ from 'jquery';
import moment from 'moment';

import AppBar from 'material-ui/AppBar';
import Divider from 'material-ui/Divider';
import FontIcon from 'material-ui/FontIcon';
import IconButton from 'material-ui/IconButton';
import LinearProgress from 'material-ui/LinearProgress';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import NavigationArrowBack from 'material-ui/svg-icons/navigation/arrow-back';
import RaisedButton from 'material-ui/RaisedButton';

import * as Constants from './Constants';
import IntInput from './FieldInput/IntInput';
import FloatInput from './FieldInput/FloatInput';
import StringInput from './FieldInput/StringInput';
import DateInput from './FieldInput/DateInput';
import TimeInput from './FieldInput/TimeInput';
import EnumInput from './FieldInput/EnumInput';
import Calendar from './Calendar';
import PointsHistory from './PointsHistory';

class SubmitPoint extends React.Component {
  constructor(props) {
    super(props);

    this.inputComponentsMap = {
      "int": IntInput,
      "float": FloatInput,
      "string": StringInput,
      "date": DateInput,
      "time": TimeInput,
      "enum": EnumInput
    };

    var visibleFieldNames = [...(props.tag.fields.map(field => field.name)), "date"];
    var point = visibleFieldNames.reduce((obj, key) => {
      obj[key] = null;
      return obj;
    }, {});

    var currentDate = moment().format(Constants.dateFormat);
    var fullTag = update(props.tag, {
      fields: {
        $push: [{
          name: "date",
          type: "date",
          defaultValue: currentDate
        }]
      }
    });
    point["date"] = currentDate;

    point["tagId"] = props.tag.id;

    this.state = {
      fullTag: fullTag,
      visibleFields: visibleFieldNames,
      point: point,
      loading: false
    };
  }

  updatePointField(fieldName, value) {
    this.setState(update(this.state, {
      point: {
        [fieldName]: {
          $set: value}
        }
      }));
  }

  goBack() {
    this.props.history.goBack();
  }
  
  submitPoint(point) {
    console.log("Submitting point", point);
    if (Object.values(this.state.point).some(value => value === null)) {
      return;
    }
    this.setState({loading: true}, () => {
      $.ajax({
        url: "/api/points",
        dataType: "json",
        contentType:"application/json; charset=utf-8",
        method: "POST",
        data: JSON.stringify(this.state.point),
        success: () => {
          this.setState({loading: false}, () => {
            this.props.notifyPointsChanged();
            this.goBack();
          });
        },
        error: (xhr, status, err) => {
          console.error(err);
          this.setState({loading: false}, () => {
            this.goBack();
          });
        }
      });
    });
  }

  render() {
    var fieldInputs = this.state.visibleFields.map(fieldName => {
      var field = this.state.fullTag.fields.find(field => field.name === fieldName);
      if (!field) {
        return;
      }
      var InputComponent = this.inputComponentsMap[field.type];
      return (
        <div key={field.name}>
          <InputComponent
            field={field}
            onSuccessfulParse={this.updatePointField.bind(this, fieldName)}
            theme={this.props.theme}
          ></InputComponent>
        </div>
      );
    });
    const style = {
      padding: "10px",
      display: "flex",
      flexDirection: "column",
      background: this.props.theme.topBackground
    };
    const marginStyle = {
      marginLeft: 8,
      marginRight: 8
    };
    const titleStyle = {
      textTransform: "uppercase",
      fontSize: "20px",
      fontWeight: "500"
    };
    const buttonStyle = {
      margin: 8
    };
    var loadingIndicator = this.state.loading
      ? <LinearProgress mode="indeterminate" />
      : null;
    return (
      <div>
        <MuiThemeProvider muiTheme={this.props.theme.headerMuiTheme}>
          <AppBar
            title={this.props.tag.id}
            titleStyle={titleStyle}
            iconElementLeft={<IconButton><NavigationArrowBack /></IconButton>}
            onLeftIconButtonTouchTap={this.goBack.bind(this)}
          />
        </MuiThemeProvider>
        <Divider />
        <div style={style}>
          <div style={marginStyle}>
            {fieldInputs}
          </div>
          <div>
            <RaisedButton
              label="Submit"
              labelPosition="before"
              primary={true}
              icon={<FontIcon className="material-icons">send</FontIcon>}
              style={buttonStyle}
              onClick={this.submitPoint.bind(this)}
            />
          </div>
          {loadingIndicator}
        </div>
        <Calendar tagId={this.props.tag.id} />
        <PointsHistory
          points={this.props.points}
          tagsById={this.props.tagsById}
          sensitiveHidden={this.props.sensitiveHidden}
          theme={this.props.theme}
          notifyPointsChanged={this.props.notifyPointsChanged}
        />
      </div>
    );
  }
}

SubmitPoint.propTypes = {
  tag: PropTypes.object,
  points: PropTypes.array,
  tagsById: PropTypes.object,
  sensitiveHidden: PropTypes.bool,
  history: PropTypes.object,
  theme: PropTypes.object,
  notifyPointsChanged: PropTypes.func
};

export default SubmitPoint;