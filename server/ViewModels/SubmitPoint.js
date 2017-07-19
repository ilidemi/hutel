import React from 'react';
import PropTypes from 'prop-types';
import moment from 'moment';
import update from 'immutability-helper';

import FlatButton from 'material-ui/FlatButton';
import FontIcon from 'material-ui/FontIcon';
import LinearProgress from 'material-ui/LinearProgress';
import RaisedButton from 'material-ui/RaisedButton';

import * as Constants from './Constants';
import IntInput from './FieldInput/IntInput';
import FloatInput from './FieldInput/FloatInput';
import StringInput from './FieldInput/StringInput';
import DateInput from './FieldInput/DateInput';
import TimeInput from './FieldInput/TimeInput';
import EnumInput from './FieldInput/EnumInput';

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
      point: point
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

  resetTag() {
    this.props.resetTag();
  }

  submitPoint() {
    if (Object.values(this.state.point).some(value => value === null)) {
      return;
    }
    this.props.submitPoint(this.state.point)
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
      flexDirection: "column"
    };
    const marginStyle = {
      marginLeft: 8,
      marginRight: 8
    };
    const titleStyle = {
      textTransform: "uppercase",
    };
    const buttonStyle = {
      margin: 8
    };
    var loadingIndicator = this.props.loading
      ? <LinearProgress mode="indeterminate" />
      : null;
    return (
      <div style={style}>
        <div>
          <FlatButton
            label="Back"
            icon={
              <FontIcon className="material-icons">
                arrow_back
              </FontIcon>}
            style={buttonStyle}
            onClick={this.resetTag.bind(this)}
          />
        </div>
        <div style={marginStyle}>
          <h1
            className="mdc-typography--title"
            style={titleStyle}
          >
            {this.props.tag.id}
          </h1>
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
    );
  }
}

SubmitPoint.propTypes = {
  loading: PropTypes.bool,
  tag: PropTypes.object,
  resetTag: PropTypes.func,
  submitPoint: PropTypes.func,
  theme: PropTypes.object
};

export default SubmitPoint;