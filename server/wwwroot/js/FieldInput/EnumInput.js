import React from 'react';
import PropTypes from 'prop-types';

import {RadioButton, RadioButtonGroup} from 'material-ui/RadioButton';

class EnumInput extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      value : this.props.field.defaultValue || "",
      validationMessage: ""
    }
  }

  onBlur(e) {
    var value = e.target.value;
    this.props.onSuccessfulParse(value);
  }

  render() {
    var radioButtonStyle = {
      margin: 8,
    };
    var radioButtonIconStyle = {
      color: this.props.theme.topText
    };
    var radioButtonLabelStyle = {
      color: this.props.theme.topText
    };
    var titleStyle = {
      color: this.props.theme.topText
    };
    var radioButtons = this.props.field.values.map(value => {
      return (
      <RadioButton
        label={value}
        labelStyle={radioButtonLabelStyle}
        iconStyle={radioButtonIconStyle}
        value={value}
        style={radioButtonStyle} />
      );
    });
    return (
      <div>
        <p
          className="mdc-typography--body1 mdc-typography--adjust-margin"
          style={titleStyle}
        >
          {this.props.field.name}
        </p>
        <RadioButtonGroup
          name={this.props.field.name}
          onChange={this.onBlur.bind(this)}>
          {radioButtons}
        </RadioButtonGroup>
      </div>
    );
  }
}

EnumInput.propTypes = {
  field: PropTypes.object,
  onSuccessfulParse: PropTypes.func,
  theme: PropTypes.object
}

export default EnumInput;