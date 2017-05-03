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
    var current = moment(this.state.value);
    if (!current.isValid()) {
      current = moment();
    }
    var result = current.add(1, 'days').toDate();
    this.update(result);
  }

  decrement() {
    var current = moment(this.state.value);
    if (!current.isValid()) {
      current = moment();
    }
    var result = current.subtract(1, 'days').toDate();
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
    return (
      <div style={style}>
        <DatePicker
          name={this.props.field.name}
          value={this.state.value}
          floatingLabelText={this.props.field.name}
          floatingLabelFixed={true}
          onChange={this.onChange.bind(this)}
        />
        <IconButton
          iconClassName="material-icons"
          style={buttonStyle}
          onClick={this.increment.bind(this)}
        >
          add
        </IconButton>
        <IconButton
          iconClassName="material-icons"
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