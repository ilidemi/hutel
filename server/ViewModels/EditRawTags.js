import React from 'react';
import PropTypes from 'prop-types';

import $ from 'jquery';

import AppBar from 'material-ui/AppBar'
import IconButton from 'material-ui/IconButton';
import LinearProgress from 'material-ui/LinearProgress';
import NavigationArrowBack from 'material-ui/svg-icons/navigation/arrow-back';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import TextField from 'material-ui/TextField';

class EditRawTags extends React.Component {
  constructor() {
    super();
    this.state = {
      value: "",
      loading: true
    };
  }

  navigateHome() {
    this.props.history.push('/');
  }

  handleChange(event) {
    this.setState({
      value: event.target.value
    });
  }

  render() {
    const style = {
      padding: 10,
      display: "flex",
      flexWrap: "wrap",
    };
    return (
      <div>
        <MuiThemeProvider muiTheme={this.props.theme.headerMuiTheme}>
          <AppBar
            title="Edit Raw Tags"
            iconElementLeft={<IconButton><NavigationArrowBack /></IconButton>}
            onLeftIconButtonTouchTap={this.navigateHome}
          />
        </MuiThemeProvider>
        {
          this.state.loading
          ?
            <div style={style}>
              <LinearProgress mode="indeterminate" />
            </div>
          :
            <TextField
              multiLine={true}
              value={this.state.value}
              onChange={this.handleChange}
            />
        }
      </div>
    );
  }
}

EditRawTags.propTypes = {
  theme: PropTypes.object,
  history: PropTypes.object
}

export default EditRawTags;