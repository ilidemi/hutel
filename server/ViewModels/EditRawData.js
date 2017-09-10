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
    }, () => {
      $.ajax({
        url: this.props.url,
        dataType: "text",
        cache: false,
        success: (data) => {
          this.setState({
            value: data,
            errorText: "",
            loading: false,
          });
        },
        error: (xhr, status, err) => {
          console.error(err);
          this.setState({
            loading: false,
            errorText: "Loading error",
          });
        }
      });
    });
  }

  submitData() {
    this.setState({
      loading: true
    },() => {
      $.ajax({
        url: this.props.url,
        contentType: "application/json",
        dataType: "text",
        method: "PUT",
        data: this.state.value,
        success: () => {
          this.setState({
            errorText: "",
            loading: false
          });
          this.redirectHome();
        },
        error: (xhr, status, err) => {
          console.error(err);
          this.setState({
            errorText: "Error",
            loading: false
          });
        }
      });
    });
  }

  handleChange(event) {
    this.setState({
      value: event.target.value
    });
  }

  goBack() {
    this.props.history.goBack();
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
            onLeftIconButtonTouchTap={this.goBack.bind(this)}
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