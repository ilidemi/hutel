import React from 'react';
import PropTypes from 'prop-types';
import moment from 'moment'

import DatePicker from 'material-ui/DatePicker';
import IconButton from 'material-ui/IconButton';

import * as Constants from '../Constants'

class DateInput extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      value: this.props.field.defaultValue
        ? moment(this.props.field.defaultValue).toDate()
        : null
    }
  }
  
  increment() {
    var result = moment(this.state.value).add(1, 'days').toDate();
    this.update(result);
  }

  decrement() {
    var result = moment(this.state.value).subtract(1, 'days').toDate();
    this.update(result);
  }

  update(value) {
    this.setState({
      value: value,
    });
    this.props.onSuccessfulParse(moment(value).format(Constants.dateFormat));
  }

  onChange(_, value) {
    this.update(value);
  }
  

  render() {
    const style = {
      display: "flex",
      flexDirection: "row",
      alignItems: "flex-end"
    }
    const buttonStyle = {
      minWidth: 36
    }
    const buttonIconStyle = {
      color: this.props.theme.topText
    }
    const floatingLabelStyle = {
      color: this.props.theme.topTextFieldHint
    }
    const floatingLabelFocusStyle = {
      color: this.props.theme.topTextFieldHintFocus
    }
    const underlineStyle = {
      borderColor: this.props.theme.topTextFieldHint
    }
    const underlineFocusStyle = {
      borderColor: this.props.theme.topTextFieldHintFocus
    }
    const inputStyle = {
      color: this.props.theme.topTextFieldInput
    }
    return (
      <div style={style}>
        <DatePicker
          name={this.props.field.name}
          value={this.state.value}
          floatingLabelText={this.props.field.name}
          floatingLabelFixed={true}
          inputStyle={inputStyle}
          floatingLabelStyle={floatingLabelStyle}
          floatingLabelFocusStyle={floatingLabelFocusStyle}
          underlineStyle={underlineStyle}
          underlineFocusStyle={underlineFocusStyle}
          onChange={this.onChange.bind(this)}
        />
        <IconButton
          iconClassName="material-icons"
          iconStyle={buttonIconStyle}
          style={buttonStyle}
          onClick={this.increment.bind(this)}
        >
          add
        </IconButton>
        <IconButton
          iconClassName="material-icons"
          iconStyle={buttonIconStyle}
          style={buttonStyle}
          onClick={this.decrement.bind(this)}
        >
          remove
        </IconButton>
      </div>
    );
  }
}

DateInput.propTypes = {
  field: PropTypes.object,
  onSuccessfulParse: PropTypes.func,
  theme: PropTypes.object
}

export default DateInput;