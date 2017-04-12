import React from 'react';
import moment from 'moment';
import * as Constants from './Constants';

class PointInput extends React.Component {
  constructor(props) {
    super(props);
    var fullTag = Object.assign({}, props.tag);
    var visibleFieldNames = [...(props.tag.fields.map(field => field.name)), "date"];
    var inputFields = visibleFieldNames.reduce((obj, key) => {
      obj[key] = {
        value: "",
        validationMessage: ""
      };
      return obj;
    }, {});
    var point = visibleFieldNames.reduce((obj, key) => {
      obj[key] = null;
      return obj;
    }, {});

    var currentDate = moment().format(Constants.dateFormat);
    fullTag.fields.push({name: "date", type: "date"});
    inputFields["date"]["value"] = currentDate;
    point["date"] = currentDate;

    point["tagId"] = props.tag.id;

    this.state = {
      fullTag: fullTag,
      visibleFields: visibleFieldNames,
      inputFields: inputFields,
      point: point
    };
  }
  
  incrementDate(fieldName) {
    var date = moment(this.state.inputFields[fieldName].value);
    if (!date.isValid()) {
      date = moment();
    }
    var result = date.add(1, 'days').format(Constants.dateFormat);
    this.parseDate(fieldName, result);
  }

  decrementDate(fieldName) {
    var date = moment(this.state.inputFields[fieldName].value);
    if (!date.isValid()) {
      date = moment();
    }
    var result = date.subtract(1, 'days').format(Constants.dateFormat);
    this.parseDate(fieldName, result);
  }

  parseInt(fieldName, value) {
    value = this.unpackBlur(value);
    var int = parseInt(Number(value));
    var success = !Number.isNaN(int);
    var result = success ? int : null;
    var validationMessage = success ? "" : `${value} is not an int`;
    this.setState({
      inputFields: Object.assign({}, this.state.inputFields, {
        [fieldName]: {
          value: value,
          validationMessage: validationMessage}
      }),
      point: Object.assign({}, this.state.point, {
        [fieldName]: result
      })
    });
  }

  parseFloat(fieldName, value) {
    value = this.unpackBlur(value);
    var float = Number(value);
    var success = Number.isFinite(float);
    var result = success ? float : null;
    var validationMessage = success ? "" : `${value} is not a float`;
    this.setState({
      inputFields: Object.assign({}, this.state.inputFields, {
        [fieldName]: {
          value: value,
          validationMessage: validationMessage}
      }),
      point: Object.assign({}, this.state.point, {
        [fieldName]: result
      })
    });
  }

  parseString(fieldName, value) {
    value = this.unpackBlur(value);
    var success = !!value.length
    var result = success ? value : null;
    var validationMessage = success ? "" : "value cannot be empty";
    this.setState({
      inputFields: Object.assign({}, this.state.inputFields, {
        [fieldName]: {
          value: value,
          validationMessage: validationMessage}
      }),
      point: Object.assign({}, this.state.point, {
        [fieldName]: result
      })
    });
  }

  parseDate(fieldName, value) {
    value = this.unpackBlur(value);
    var date = moment(value);
    var success = date.isValid();
    var result = success ? date.format(Constants.dateFormat) : null;
    var validationMessage = success ? "" : `${value} is not a date`;
    this.setState({
      inputFields: Object.assign({}, this.state.inputFields, {
        [fieldName]: {
          value: value,
          validationMessage: validationMessage}
      }),
      point: Object.assign({}, this.state.point, {
        [fieldName]: result
      })
    });
  }

  parseTime(fieldName, value) {
    value = this.unpackBlur(value);
    var time = moment(value, Constants.timeFormat);
    var success = time.isValid();
    var result = success ? time.format(Constants.timeFormat) : null;
    var validationMessage = success ? "" : `${value} is not a time`;
    this.setState({
      inputFields: Object.assign({}, this.state.inputFields, {
        [fieldName]: {
          value: value,
          validationMessage: validationMessage}
      }),
      point: Object.assign({}, this.state.point, {
        [fieldName]: result
      })
    });
  }

  parseEnum(fieldName, value) {
    value = this.unpackBlur(value);
    var field = this.state.fullTag.fields.find(field => field.name === fieldName);
    var success = field.values.includes(value);
    var result = success ? value : null;
    var validationMessage = success ? "" : `${value} doesn't belong to this enum`;
    this.setState({
      inputFields: Object.assign({}, this.state.inputFields, {
        [fieldName]: {
          value: value,
          validationMessage: validationMessage}
      }),
      point: Object.assign({}, this.state.point, {
        [fieldName]: result
      })
    });
  }

  unpackBlur(value) {
    return value.type === "blur" ? value.target.value : value;
  }

  updateField(fieldName, e) {
    this.setState({
      inputFields: Object.assign({}, this.state.inputFields, {
        [fieldName]: {
          value: e.target.value,
          validationMessage: ""
        }
      })
    })
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
      var input;
      var field = this.state.fullTag.fields.find(field => field.name === fieldName);
      if (!field) {
        return;
      }
      switch(field.type) {
        case "int":
          input = <input
            onBlur={this.parseInt.bind(this, fieldName)}
            value={this.state.inputFields[fieldName].value}
            onChange={this.updateField.bind(this, fieldName)}
          ></input>;
          break;
        case "float":
          input = <input
            onBlur={this.parseFloat.bind(this, fieldName)}
            value={this.state.inputFields[fieldName].value}
            onChange={this.updateField.bind(this, fieldName)}
          ></input>;
          break;
        case "string":
          input = <input
            onBlur={this.parseString.bind(this, fieldName)}
            value={this.state.inputFields[fieldName].value}
            onChange={this.updateField.bind(this, fieldName)}
          ></input>;
          break;
        case "date":
          input = (
            <div>
              <input
                onBlur={this.parseDate.bind(this, fieldName)}
                value={this.state.inputFields[fieldName].value}
                onChange={this.updateField.bind(this, fieldName)}
              ></input>
              <button onClick={this.incrementDate.bind(this, fieldName)}>+</button>
              <button onClick={this.decrementDate.bind(this, fieldName)}>-</button>
            </div>
          );
          break;
        case "time":
          input = <input
            onBlur={this.parseTime.bind(this, fieldName)}
            value={this.state.inputFields[fieldName].value}
            onChange={this.updateField.bind(this, fieldName)}
          ></input>;
          break;
        case "enum":
          input = (
            <div>
              | {field.values.join(" | ")} |
              <input
                onBlur={this.parseEnum.bind(this, fieldName)}
                value={this.state.inputFields[fieldName].value}
                onChange={this.updateField.bind(this, fieldName)}
              ></input>
            </div>
          );
          break;
      }
      return (
        <div>
          {fieldName}
          {input}
          {this.state.inputFields[fieldName].validationMessage}
        </div>
      );
    });
    return (
      <div>
        <button onClick={this.resetTag.bind(this)}>⬅ Back</button>
        <div>{this.props.tag.id}</div>
        {fieldInputs}
        <button onClick={this.submitPoint.bind(this)}>Submit</button>
      </div>
    );
  }
}

export default PointInput;