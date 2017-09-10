import React from 'react';
import PropTypes from 'prop-types';

import TextField from 'material-ui/TextField';

class FloatInput extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      value : this.props.field.defaultValue || "",
      validationMessage: ""
    };
  }

  onBlur(e) {
    var value = e.target.value;
    var float = Number(value);
    var success = value && Number.isFinite(float);
    var validationMessage = success ? "" : `"${value}" is not a float`;
    this.setState({
      value: value,
      validationMessage: validationMessage
    });
    if (success) {
      this.props.onSuccessfulParse(float);
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
          type="number"
          floatingLabelText={this.props.field.name}
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

FloatInput.propTypes = {
  field: PropTypes.object,
  onSuccessfulParse: PropTypes.func,
  theme: PropTypes.object
};

export default FloatInput;