import React from 'react';
import injectTapEventPlugin from 'react-tap-event-plugin';

import {Route} from 'react-router-dom';

import * as Colors from 'material-ui/styles/colors';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import getMuiTheme from 'material-ui/styles/getMuiTheme';
import Paper from 'material-ui/Paper';

import Home from './Home';

injectTapEventPlugin();

class App extends React.Component {
  constructor(){
    super();
  }

  render() {
    const theme = {
      headerBackground: Colors.deepPurple900,
      headerText: Colors.fullWhite,
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
        </Paper>
      </MuiThemeProvider>
    );
  }
}

export default App;
