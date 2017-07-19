import React from 'react';
import injectTapEventPlugin from 'react-tap-event-plugin';

import { HashRouter, Route } from 'react-router-dom';

import * as Colors from 'material-ui/styles/colors';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import getMuiTheme from 'material-ui/styles/getMuiTheme';
import Paper from 'material-ui/Paper';

import EditRawTags from './EditRawTags';
import Home from './Home';

injectTapEventPlugin();

const App = () => {
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
  return (
    <HashRouter>
      <MuiThemeProvider muiTheme={muiTheme}>
        <Paper
          style={columnStyle}
          zDepth={5}
        >
          <div>
            <Route exact path="/" render={(props) => (
              <Home {...props} theme={theme} />
            )} />
          </div>
          <div>
            <Route exact path="/edit/tags" render={(props) => (
              <EditRawTags {...props} theme={theme} />
            )} />
          </div>
        </Paper>
      </MuiThemeProvider>
    </HashRouter>
  );
};

export default App;
