import React from 'react';
import PropTypes from 'prop-types';

import LinearProgress from 'material-ui/LinearProgress';
import FontIcon from 'material-ui/FontIcon';
import RaisedButton from 'material-ui/RaisedButton';

class SelectTag extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      isExpanded: false
    };
  }

  selectTag(tagId) {
    this.props.selectTag(tagId);
  }

  expand() {
    this.setState({
      isExpanded: true
    });
  }
  
  collapse() {
    this.setState({
      isExpanded: false
    });
  }

  render() {
    const style = {
      padding: 10,
      display: "flex",
      flexWrap: "wrap",
    };

    if (this.props.loading) {
      return (
        <div style={style}>
          <LinearProgress mode="indeterminate" />
        </div>
      );
    } else {
      const buttonStyle = {
        margin: 8
      };
      const iconStyle = {
        fontSize: 20
      }
      const labelStyle = {
        paddingLeft: 6
      }
      var toButton = tag => (
        <RaisedButton
          key={'+' + tag.id}
          onClick={this.selectTag.bind(this, tag.id)}
          label={tag.id}
          labelStyle={labelStyle}
          primary={true}
          style={buttonStyle}
          icon={
            <FontIcon
              className="material-icons"
              style={iconStyle}
            >
              add
            </FontIcon>
          }
        />
      );
      var expandButton =
        <RaisedButton
          key="expand"
          onClick={this.expand.bind(this)}
          primary={true}
          style={buttonStyle}
          icon={
            <FontIcon
              className="material-icons"
              style={iconStyle}
            >
              expand_more
            </FontIcon>
          }
        />
      var collapseButton =
        <RaisedButton
          key="collapse"
          onClick={this.collapse.bind(this)}
          primary={true}
          style={buttonStyle}
          icon={
            <FontIcon
              className="material-icons"
              style={iconStyle}
            >
              expand_less
            </FontIcon>
          }
        />
      var buttons = this.state.isExpanded
        ? this.props.tags.map(toButton).concat([collapseButton])
        : this.props.tags.slice(0, 10).map(toButton).concat([expandButton]);
      return (
        <div style={style}>
          {buttons}
        </div>
      );
    }
  }
}

SelectTag.propTypes = {
  loading: PropTypes.bool,
  tags: PropTypes.array.isRequired,
  selectTag: PropTypes.func,
  theme: PropTypes.object
}

export default SelectTag;