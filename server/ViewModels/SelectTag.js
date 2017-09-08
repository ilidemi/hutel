import React from 'react';
import PropTypes from 'prop-types';

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
    this.props.history.push('/submit/' + tagId);
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
      background: this.props.theme.topBackground
    };

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
      ? this.props.tags.map(toButton).concat(
          this.props.tags.length >= 10
            ? [collapseButton]
            : [])
      : this.props.tags.slice(0, 10).map(toButton).concat(
          this.props.tags.length >= 10
            ? [expandButton]
            : []);
    return (
      <div style={style}>
        {buttons}
      </div>
    );
  }
}

SelectTag.propTypes = {
  tags: PropTypes.array.isRequired,
  history: PropTypes.object,
  theme: PropTypes.object
}

export default SelectTag;