import React from 'react';
import PropTypes from 'prop-types';
import moment from 'moment';

import TextField from 'material-ui/TextField';

import * as Constants from '../Constants';

class TimeInput extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      value : this.props.field.defaultValue
        ? moment(this.props.field.defaultValue, Constants.timeFormat)
        : null
    };
  }
  
  onBlur(e) {
    var value = e.target.value;
    var current = moment(value, Constants.timeFormat);
    var success = current.isValid();
    var validationMessage = success ? "" : "The format is " + Constants.timeFormat;
    this.setState({
      value: value,
      validationMessage: validationMessage
    });
    if (success) {
      this.props.onSuccessfulParse(value);
    }
  }

  onChange(e) {
    this.setState({
      value: e.target.value,
      validationMessage: ""
    });
  }

  render() {
    return (
      <div>
        <TextField
          name={this.props.field.name}
          floatingLabelText={this.props.field.name + " (h:mm:ss)"}
          floatingLabelFixed={true}
          value={this.state.value}
          errorText={this.state.validationMessage}
          onBlur={this.onBlur.bind(this)}
          onChange={this.onChange.bind(this)}
        ></TextField>
      </div>
    );
  }
}

TimeInput.propTypes = {
  field: PropTypes.object,
  onSuccessfulParse: PropTypes.func,
  theme: PropTypes.object
};

export default TimeInput;