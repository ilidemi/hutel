import React from 'react';
import injectTapEventPlugin from 'react-tap-event-plugin';
import { HashRouter, Route } from 'react-router-dom';

import $ from 'jquery';
import moment from 'moment';

import * as Colors from 'material-ui/styles/colors';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import getMuiTheme from 'material-ui/styles/getMuiTheme';
import Paper from 'material-ui/Paper';

import * as Constants from './Constants'
import EditRawData from './EditRawData';
import Home from './Home';
import Loading from './Loading';
import SubmitPoint from './SubmitPoint';

injectTapEventPlugin();

class App extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      tags: [],
      points: [],
      tagsLoading: true,
      pointsLoading: true
    }
  }

  componentDidMount() {
    this.updatePoints();
    this.updateTags();
  }

  updatePoints() {
    this.setState({pointsLoading: true}, function(){
      console.log(this.state);
    });
    $.ajax({
      url: "/api/points",
      data: {
        startDate: moment().subtract(7, 'days').format(Constants.dateFormat)
      },
      dataType:'json',
      cache: false,
      success: function(data){
        this.setState({points: data, pointsLoading: false}, function(){
          console.log(this.state);
        });
      }.bind(this),
      error: function(xhr, status, err){
        console.log(err);
        this.setState({pointsLoading: false}, function(){
          console.log(this.state);
        });
      }.bind(this)
    });
  }

  updateTags() {
    this.setState({tagsLoading: true}, function() {
      console.log(this.state);
    });
    $.ajax({
      url: "/api/tags",
      dataType: "json",
      cache: false,
      success: function(data) {
        this.setState({tags: data, tagsLoading: false}, function() {
          console.log(this.state);
        });
      }.bind(this),
      error: function(xhr, status, err) {
        console.log(err);
        this.setState({tagsLoading: false}, function() {
          console.log(this.state);
        });
      }.bind(this)
    });
  }

  render() {
    const theme = {
      headerBackground: Colors.deepPurple900,
      headerText: Colors.fullWhite,
      headerMuiTheme: getMuiTheme({
        palette: {
          primary1Color: Colors.deepPurple900
        }
      }),
      topBackground: Colors.fullWhite,
      historyBackground: Colors.grey100,
      historyDateText: Colors.deepPurple900
    }
    const muiTheme = getMuiTheme({
      palette: {
        primary1Color: Colors.amber900,
        accent1Color: Colors.yellow500
      }
    });
    const columnStyle = {
      maxWidth: 800,
      margin: "auto",
      display: "flex",
      flexDirection: "column",
      flexHeight: "100%",
      flexMinHeight: "100%"
    };
    const body = this.state.tagsLoading || this.state.pointsLoading
      ? <Loading theme={theme} />
      : <div>
          <div>
            <Route exact path="/" render={(props) => (
              <Home
                {...props}
                tags={this.state.tags}
                points={this.state.points}
                theme={theme}
              />
            )} />
          </div>
          <div>
            <Route path="/submit/:tag" render={(props) => (
              <SubmitPoint
                {...props}
                tag={this.state.tags.find(tag => tag.id === props.match.params.tag)}
                theme={theme}
                points={this.state.points}
                updatePoints={this.updatePoints.bind(this)}
              />
            )} />
          </div>
          <div>
            <Route exact path="/edit/tags" render={(props) => (
              <EditRawData
                {...props}
                theme={theme}
                url={"/api/tags"}
                title={"Edit Raw Tags"}
                floatingLabel={"Tags"}
              />
            )} />
          </div>
          <div>
            <Route exact path="/edit/points" render={(props) => (
              <EditRawData
                {...props}
                theme={theme}
                url={"/api/points"}
                title={"Edit Raw Points"}
                floatingLabel={"Points"}
              />
            )} />
          </div>
        </div>
    return (
      <HashRouter>
        <MuiThemeProvider muiTheme={muiTheme}>
          <Paper
            style={columnStyle}
            zDepth={5}
          >
          {body}
          </Paper>
        </MuiThemeProvider>
      </HashRouter>
    );
  }
}

export default App;
