import React from 'react';
import PropTypes from 'prop-types';

import $ from 'jquery';

import AppBar from 'material-ui/AppBar'
import FontIcon from 'material-ui/FontIcon';
import IconButton from 'material-ui/IconButton';
import LinearProgress from 'material-ui/LinearProgress';
import RaisedButton from 'material-ui/RaisedButton';
import NavigationArrowBack from 'material-ui/svg-icons/navigation/arrow-back';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import TextField from 'material-ui/TextField';

class EditRawTags extends React.Component {
  constructor() {
    super();
    this.state = {
      value: "",
      errorText: "",
      loading: true,
    };
  }

  componentDidMount() {
    this.updateTags();
  }

  updateTags() {
    this.setState({
      loading: true,
      submitButtonEnabled: false
    }, function() {
      console.log(this.state);
    });
    $.ajax({
      url: "/api/tags",
      dataType: "text",
      cache: false,
      success: function(data) {
        this.setState({
          value: data,
          errorText: "",
          loading: false,
        }, function() {
          console.log(this.state);
        });
      }.bind(this),
      error: function(xhr, status, err) {
        console.log(err);
        this.setState({
          loading: false,
          errorText: "Loading error",
        }, function() {
          console.log(this.state);
        });
      }.bind(this)
    });
  }

  submitTags() {
    this.setState({
      loading: true
    }, function() {
      console.log(this.state);
    });
    $.ajax({
      url: "/api/tags",
      dataType: "text",
      method: "POST",
      success: function() {
        this.setState({
          errorText: "",
          loading: false
        }, function() {
          console.log(this.state);
        });
        this.redirectHome();
      }.bind(this),
      error: function(xhr, status, err) {
        console.log(err);
        this.setState({
          errorText: "Error",
          loading: false
        }, function() {
          console.log(this.state);
        });
      }.bind(this)
    });
  }

  handleChange(event) {
    this.setState({
      value: event.target.value
    });
  }

  redirectHome() {
    this.props.history.push('/');
  }

  render() {
    const style = {
      padding: 10,
      display: "flex",
      flexWrap: "wrap",
    };
    const buttonStyle = {
      margin: 8
    };

    return (
      <div>
        <MuiThemeProvider muiTheme={this.props.theme.headerMuiTheme}>
          <AppBar
            title="Edit Raw Tags"
            iconElementLeft={<IconButton><NavigationArrowBack /></IconButton>}
            onLeftIconButtonTouchTap={this.redirectHome.bind(this)}
          />
        </MuiThemeProvider>
        <div style={style}>
          {
            this.state.loading
            ?
              <LinearProgress mode="indeterminate" />
            :
              <div>
                <TextField
                  multiLine={true}
                  floatingLabelText="Tags"
                  floatingLabelFixed={true}
                  fullWidth={true}
                  value={this.state.value}
                  onChange={this.handleChange}
                />
                <RaisedButton
                  label="Submit"
                  labelPosition="before"
                  primary={true}
                  icon={<FontIcon className="material-icons">send</FontIcon>}
                  style={buttonStyle}
                  onClick={this.submitTags.bind(this)}
                />
              </div>
          }
        </div>
      </div>
    );
  }
}

EditRawTags.propTypes = {
  theme: PropTypes.object,
  history: PropTypes.object
}

export default EditRawTags;