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

class EditRawData extends React.Component {
  constructor() {
    super();
    this.state = {
      value: "",
      errorText: "",
      loading: true,
    };
  }

  componentDidMount() {
    this.updateData();
  }

  updateData() {
    this.setState({
      loading: true,
      submitButtonEnabled: false
    }, function() {
      console.log(this.state);
    });
    $.ajax({
      url: this.props.url,
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

  submitData() {
    this.setState({
      loading: true
    }, function() {
      console.log(this.state);
    });
    $.ajax({
      url: this.props.url,
      contentType: "application/json",
      dataType: "text",
      method: "PUT",
      data: this.state.value,
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
      padding: "8px 24px",
      display: "flex",
      flexWrap: "wrap",
      width: "100%",
      boxSizing: "border-box"
    };
    const buttonStyle = {
      margin: 8
    };
    const textareaStyle = {
      fontFamily: "'Roboto Mono', monospace",
      fontSize: "14px",
      lineHeight: "normal"
    }

    return (
      <div>
        <MuiThemeProvider muiTheme={this.props.theme.headerMuiTheme}>
          <AppBar
            title={this.props.title}
            iconElementLeft={<IconButton><NavigationArrowBack /></IconButton>}
            onLeftIconButtonTouchTap={this.redirectHome.bind(this)}
          />
        </MuiThemeProvider>
          {
            this.state.loading
            ?
              <div style={style}>
                <LinearProgress mode="indeterminate" />
              </div>
            :
              <div style={style}>
                <TextField
                  multiLine={true}
                  floatingLabelText={this.props.floatingLabel}
                  floatingLabelFixed={true}
                  fullWidth={true}
                  textareaStyle={textareaStyle}
                  value={this.state.value}
                  onChange={this.handleChange.bind(this)}
                />
                <RaisedButton
                  label="Submit"
                  labelPosition="before"
                  primary={true}
                  icon={<FontIcon className="material-icons">send</FontIcon>}
                  style={buttonStyle}
                  onClick={this.submitData.bind(this)}
                />
              </div>
          }
      </div>
    );
  }
}

EditRawData.propTypes = {
  theme: PropTypes.object,
  history: PropTypes.object,
  url: PropTypes.string,
  title: PropTypes.string,
  floatingLabel: PropTypes.string
}

export default EditRawData;