import React from 'react';
import PropTypes from 'prop-types';
import moment from 'moment';

import TimePicker from 'material-ui/TimePicker';

import * as Constants from '../Constants'

class TimeInput extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      value : this.props.field.defaultValue
        ? moment(this.props.field.defaultValue, Constants.timeFormat)
        : null
    }
  }

  onChange(_, value) {
    this.setState({
      value: value,
    });
    this.props.onSuccessfulParse(moment(value).format(Constants.timeFormat));
  }

  render() {
    return (
      <div>
        <TimePicker
          format="24hr"
          floatingLabelText={this.props.field.name}
          floatingLabelFixed={true}
          value={this.state.value}
          onChange={this.onChange.bind(this)}
        />
      </div>
    );
  }
}

TimeInput.propTypes = {
  field: PropTypes.object,
  onSuccessfulParse: PropTypes.func,
  theme: PropTypes.object
}

export default TimeInput;