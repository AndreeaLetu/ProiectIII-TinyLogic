
if (typeof Blockly === "undefined") {
    throw new Error("Blockly not loaded!");
}

const JS = Blockly.JavaScript;



Blockly.defineBlocksWithJsonArray([

    {
        "type": "tl_on_start",
        "message0": "Când apeși pe ✓ %1",
        "args0": [{ "type": "input_statement", "name": "DO" }],
        "colour": 120,
        "hat": "cap"
    },

    {
        "type": "tl_move",
        "message0": "mută %1 pași",
        "args0": [{
            "type": "field_number",
            "name": "STEPS",
            "value": 10,
            "min": 1
        }],
        "previousStatement": null,
        "nextStatement": null,
        "colour": 210
    },

  
    {
        "type": "tl_turn_right",
        "message0": "rotește dreapta %1°",
        "args0": [{
            "type": "field_number",
            "name": "DEG",
            "value": 90
        }],
        "previousStatement": null,
        "nextStatement": null,
        "colour": 210
    },

    {
        "type": "tl_turn_left",
        "message0": "rotește stânga %1°",
        "args0": [{
            "type": "field_number",
            "name": "DEG",
            "value": 90
        }],
        "previousStatement": null,
        "nextStatement": null,
        "colour": 210
    },

   
    {
        "type": "tl_set_var",
        "message0": "setează %1 la %2",
        "args0": [
            { "type": "field_input", "name": "VAR", "text": "scor" },
            { "type": "field_number", "name": "VAL", "value": 0 }
        ],
        "previousStatement": null,
        "nextStatement": null,
        "colour": 330
    },

    {
        "type": "tl_change_var",
        "message0": "schimbă %1 cu %2",
        "args0": [
            { "type": "field_input", "name": "VAR", "text": "scor" },
            { "type": "field_number", "name": "VAL", "value": 1 }
        ],
        "previousStatement": null,
        "nextStatement": null,
        "colour": 330
    }

]);



JS.forBlock['tl_on_start'] = function (block) {
    const body = JS.statementToCode(block, 'DO')
        .trim()
        .replace(/;+$/, '');
    return `start{${body}}`;
};


JS.forBlock['tl_move'] = function (block) {
    return `move:${block.getFieldValue("STEPS")};`;
};


JS.forBlock['tl_turn_right'] = function (block) {
    return `turn:right:${block.getFieldValue("DEG")};`;
};


JS.forBlock['tl_turn_left'] = function (block) {
    return `turn:left:${block.getFieldValue("DEG")};`;
};

JS.forBlock['tl_set_var'] = function (block) {
    return `set:${block.getFieldValue("VAR")}:${block.getFieldValue("VAL")};`;
};


JS.forBlock['tl_change_var'] = function (block) {
    const v = block.getFieldValue("VAL");
    const sign = v >= 0 ? "+" : "";
    return `change:${block.getFieldValue("VAR")}:${sign}${v};`;
};


JS.forBlock['controls_repeat_ext'] = function (block) {
    const times = JS.valueToCode(block, 'TIMES', JS.ORDER_NONE) || '0';
    let body = JS.statementToCode(block, 'DO')
        .trim()
        .replace(/;+$/, '');
    return `repeat:${times}{${body}};`;
};

window.getTinyLogicCode = function (workspace) {
    let code = Blockly.JavaScript.workspaceToCode(workspace);
    return code
        .replace(/\s+/g, "")
        .replace(/^start\{/, "")
        .replace(/\}$/, "")
        .replace(/;+$/, "");
};
